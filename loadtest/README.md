# Testes de carga (load/stress)

Harness para medir o comportamento da API sob carga **multi-tenant**, com banco
populado de forma realista. Primeiro corte: **cenário de leitura** (`k6`).

## Componentes

| Item | O quê |
|---|---|
| `tools/PetHotel.LoadTest.Seeder` | Seeder em C# (Bogus) que insere dados realistas pelos DbContexts reais e emite 1 JWT por tenant. Gera `loadtest/manifest.json`. |
| `loadtest/k6/read-heavy.js` | Cenário k6 de leitura (listas, busca, ficha, ocupação), ponderado por porte de tenant, com thresholds (SLO). |
| `loadtest/manifest.json` | Saída do seeder (tokens + amostras de ids). **Não versionado.** |

## Pré-requisitos

- Postgres de pé (o `docker-compose.yml` do projeto serve).
- [k6](https://grafana.com/docs/k6/latest/set-up/install-k6/) instalado.
- **Banco dedicado** `pethotel_loadtest` (não polua o dev).

## Passo a passo

### 1. Banco de carga (uma vez)

```bash
docker exec pethotel-pg psql -U postgres -c "CREATE DATABASE pethotel_loadtest"

# aplica as migrations dos 4 contextos (o factory lê a env PETHOTEL_DB)
export PETHOTEL_DB="Host=localhost;Port=5432;Database=pethotel_loadtest;Username=postgres;Password=dev"
for ctx in \
  "Registry/PetHotel.Registry.Infrastructure:RegistryDbContext" \
  "Health/PetHotel.Health.Infrastructure:HealthDbContext" \
  "Booking/PetHotel.Booking.Infrastructure:BookingDbContext" \
  "Tenancy/PetHotel.Tenancy.Infrastructure:TenancyDbContext"; do
  dotnet ef database update --project "src/Modules/${ctx%%:*}" \
    --startup-project src/PetHotel.Api --context "${ctx##*:}"
done
```

### 2. Semear dados

```bash
export LOADTEST_CONNECTION="Host=localhost;Port=5432;Database=pethotel_loadtest;Username=postgres;Password=dev"
dotnet run --project tools/PetHotel.LoadTest.Seeder
```

Volume configurável por env:

| Env | Default | O quê |
|---|---|---|
| `SEED_SCALE` | `1.0` | Multiplicador de volume (use `5` para ~5x, `0.1` para um smoke). |
| `LOADTEST_CONNECTION` | dev local | String de conexão. |
| `JWT_SIGNING_KEY` / `JWT_ISSUER` / `JWT_AUDIENCE` | dev | Devem bater com os do `appsettings` da API testada. |

> Re-semear acrescenta tenants. Para zerar: `TRUNCATE registry.pets, registry.tutors, booking.reservations, booking.accommodations, health.health_records, health.vaccinations, health.parasite_treatments CASCADE;`

### 3. Subir a API apontando para o banco de carga

Use o **profile `loadtest`** — ele já aponta para `pethotel_loadtest`, casa o JWT com o
seeder, reduz o log e ajusta o pool (via `appsettings.LoadTest.json`). **Em Release**
(não use o build de debug do Rider para medir):

```bash
dotnet run -c Release --project src/PetHotel.Api --launch-profile loadtest
```

No Rider: selecione o profile **loadtest** e rode em configuração **Release**.

> Parâmetros do ambiente ficam em `src/PetHotel.Api/appsettings.LoadTest.json`
> (connection string + `Maximum Pool Size` como knob de tuning, JWT, logging). Fora de
> `Development` o Swagger não sobe — use `/health` para checar prontidão.

### 4. Rodar o cenário

```bash
k6 run loadtest/k6/read-heavy.js
# ou apontando para outro host:
BASE_URL=http://localhost:5131 k6 run loadtest/k6/read-heavy.js
```

O teste **falha** (exit ≠ 0) se algum threshold estourar — pronto para CI.

## Como ler o resultado

- `http_req_duration{name:...}` → p95/p99 por endpoint. Os suspeitos:
  - `search_pets` (busca `ILike '%x%'` sem índice trigram) → tende a escalar mal.
  - `occupancy` (overlap de datas sem índice de data) e listas (sort por `CreatedAt` sem índice).
- `http_req_failed` → erros/timeouts (inclui exaustão de pool de conexão).

Correlacione com o lado do banco enquanto roda:

```sql
-- top queries por tempo total (precisa de pg_stat_statements habilitado)
SELECT calls, mean_exec_time, total_exec_time, query
FROM pg_stat_statements ORDER BY total_exec_time DESC LIMIT 20;

-- conexões/espera (saturação do pool aparece aqui)
SELECT state, wait_event_type, count(*) FROM pg_stat_activity GROUP BY 1,2;

-- plano de uma query quente (confirma seq scan vs index)
EXPLAIN (ANALYZE, BUFFERS) SELECT * FROM registry.pets
WHERE "TenantId" = '<id>' ORDER BY "CreatedAt" DESC LIMIT 20;
```

## Próximos cenários (planejados)

- **Escrita**: criar reserva, check-in/out, registrar vacina (mede custo do Outbox por escrita).
- **Concorrência**: confirms simultâneos na mesma acomodação (xmin/overbooking) — mede contention e taxa de 409.
- **Observabilidade**: apontar o OpenTelemetry da API para Prometheus/Grafana/Tempo e correlacionar latência com traces/queries.
