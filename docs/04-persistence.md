# 04 — Persistência (EF Core + PostgreSQL)

Provider **Npgsql**. Um `DbContext` por módulo, schema próprio por módulo (`booking.*`, `health.*`).

## Multi-tenancy (banco compartilhado + `TenantId`)

### Resolução de tenant
- `ITenantContext` com escopo de request, populado a partir do **token de autenticação**.
- **Nunca** ler `TenantId` de parâmetro de URL/query string.

### Global Query Filter (leitura isolada)
Toda entidade com tenant implementa `IHasTenant { TenantId TenantId }` e recebe filtro:

```csharp
modelBuilder.Entity<Reservation>()
    .HasQueryFilter(e => e.TenantId == _tenantContext.Current);
```

### Carimbo automático (escrita isolada)
Um `SaveChangesInterceptor` preenche `TenantId` em toda inserção de `IHasTenant`, evitando vazamento por esquecimento. O mesmo interceptor preenche auditoria (`CreatedAt/By`, `UpdatedAt/By`).

### Evolução
Tenant grande pode ir para **banco dedicado** mudando só a resolução da connection string (`ITenantConnectionResolver`). O domínio não muda. Manter essa porta aberta desde já.

## Concorrência otimista (PostgreSQL)

Usar a coluna de sistema `xmin` como token de concorrência — sem coluna extra:

```csharp
modelBuilder.Entity<Reservation>().UseXminAsConcurrencyToken();
modelBuilder.Entity<Accommodation>().UseXminAsConcurrencyToken();
```

Crítico em `Reservation`/`Accommodation` para impedir **overbooking** sob concorrência. Em `DbUpdateConcurrencyException`, traduzir para `Error.Conflict` e deixar o chamador decidir retry.

## Unit of Work

- O `DbContext` **é** a Unit of Work; `SaveChangesAsync` é o limite transacional de um agregado.
- Repositórios expõem apenas operações de agregado (`FindAsync`, `Add`, sem `IQueryable` vazando para fora).
- **DbContext pooling** (`AddDbContextPool`) para reduzir alocação sob carga.
- Outbox grava na **mesma** transação do agregado (ver `docs/05`).

## Leitura (CQRS-lite)

O lado de leitura **não** passa pelos agregados:
- Projeção direta para DTO via `Select` + `AsNoTracking`.
- **Paginação por keyset/cursor** (não `OFFSET`) em listas grandes: calendário de ocupação, histórico do diário de bordo.
- `AsSplitQuery` quando `Include` gerar produto cartesiano.
- Compiled queries para as consultas quentes e repetitivas.
- O **calendário de ocupação** é a tela mais consultada — considerar read model dedicado e índices específicos.

## Mapeamento (Fluent API)

- Mapear via `IEntityTypeConfiguration<T>` por entidade, em `Infrastructure/Persistence/Configurations`.
- **Sem data annotations no domínio** (o domínio não conhece EF).
- Identificadores tipados (`ReservationId`) mapeados com value converter.
- Value Objects: `OwnsOne`/`ComplexProperty` conforme o caso.

## Migrations

- Cada módulo tem suas migrations (DbContext próprio) → evolução independente.
- **Não rodar `MigrateAsync` no startup em produção** (corrida entre instâncias).
- Aplicar via **migration bundle** ou passo dedicado de *migrator* no pipeline de deploy; scripts idempotentes.
- Comando por módulo, ex.:
  ```
  dotnet ef migrations add <Nome> \
    --project src/Modules/Booking/PetHotel.Booking.Infrastructure \
    --startup-project src/PetHotel.Api \
    --context BookingDbContext
  ```

## Índices e auditoria

- Índice em `TenantId` + colunas de filtro frequentes em cada tabela tenant.
- Soft-delete via flag + query filter combinado com o de tenant.
- Auditoria preenchida por interceptor, atendendo ao requisito de log auditável.
