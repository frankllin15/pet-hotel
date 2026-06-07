# 07 — Getting Started

Passo a passo para sair do zero até o núcleo navegável (Tenancy → Registry → Health → Booking). Comandos como referência; ajuste nomes conforme necessário.

## Pré-requisitos

- SDK do **.NET 10**.
- **Docker** (PostgreSQL local e Testcontainers).
- Ferramenta EF: `dotnet tool install --global dotnet-ef`.

## 1. Solução e estrutura base

```bash
dotnet new sln -n PetHotel

# BuildingBlocks
dotnet new classlib -o src/BuildingBlocks/PetHotel.SharedKernel
dotnet new classlib -o src/BuildingBlocks/PetHotel.BuildingBlocks

# Host
dotnet new web -o src/PetHotel.Api
```

Adicionar todos os projetos ao `.sln` (`dotnet sln add <csproj>`).

## 2. Um módulo (repetir o padrão por bounded context)

```bash
M=Booking
dotnet new classlib -o src/Modules/$M/PetHotel.$M.Domain
dotnet new classlib -o src/Modules/$M/PetHotel.$M.Application
dotnet new classlib -o src/Modules/$M/PetHotel.$M.Infrastructure
```

Referências respeitando a regra de dependência (`docs/01`):
- `Application` → `Domain`, `SharedKernel`
- `Infrastructure` → `Application`
- `Api` → `Infrastructure` de cada módulo (apenas para registrar DI)

## 3. Pacotes principais

No host e/ou Infrastructure conforme o papel:

```bash
# EF Core + PostgreSQL
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design

# Wolverine (mediator + outbox)
dotnet add package WolverineFx
dotnet add package WolverineFx.Http
dotnet add package WolverineFx.EntityFrameworkCore
dotnet add package WolverineFx.Postgresql

# Validação e mapeamento
dotnet add package FluentValidation
dotnet add package Riok.Mapperly

# Observabilidade e resiliência
dotnet add package Serilog.AspNetCore
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package Microsoft.Extensions.Http.Resilience
```

Testes:

```bash
dotnet add package xunit
dotnet add package Testcontainers.PostgreSql
dotnet add package NetArchTest.Rules
```

> **Não** adicionar `MediatR` nem `AutoMapper` (comerciais desde 2025). Wolverine cobre o mediator; Mapperly cobre o mapeamento.

## 4. PostgreSQL local

```bash
docker run --name pethotel-pg -e POSTGRES_PASSWORD=dev \
  -e POSTGRES_DB=pethotel -p 5432:5432 -d postgres:17
```

## 5. Ordem de implementação do núcleo

1. **SharedKernel:** `Entity`, `AggregateRoot`, `ValueObject`, `IDomainEvent`, `Result<T>`, `Error`, identificadores tipados. (`docs/03`)
2. **BuildingBlocks:** `ITenantContext`, `SaveChangesInterceptor` (tenant + auditoria), base de Outbox via Wolverine. (`docs/04`, `docs/05`)
3. **Tenancy:** `Tenant`, `User`, `TenantConfiguration` + resolução de tenant a partir do token.
4. **Registry:** `Tutor`, `Pet` + DbContext + migrations + endpoints de cadastro.
5. **Health:** `HealthRecord` (vacinas com validade) + contrato público de *clearance*.
6. **Booking:** `Reservation`, `Accommodation`; `Confirm` consome o clearance de Health; concorrência via `xmin`; calendário de ocupação como query projetada.

## 6. Wolverine + EF Core no host

Seguir o padrão de configuração em `docs/05` (`UseWolverine`, `PersistMessagesWithPostgresql`, `UseEntityFrameworkCoreTransactions`, `AutoApplyTransactions`).

## 7. Migrations (por módulo)

```bash
dotnet ef migrations add Initial \
  --project src/Modules/Booking/PetHotel.Booking.Infrastructure \
  --startup-project src/PetHotel.Api \
  --context BookingDbContext
```

Em produção, aplicar via migration bundle / passo de migrator no pipeline — **não** no startup. (`docs/04`)

## 8. Primeiro fluxo de ponta a ponta (critério de "núcleo pronto")

Cadastrar tutor → cadastrar pet → registrar vacina → criar reserva → **confirmar reserva bloqueada por vacina vencida** → confirmar com vacina válida → ver pet no calendário de ocupação. Cobrir esse fluxo com teste de integração (`docs/06`).
