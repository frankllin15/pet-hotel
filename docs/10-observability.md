# 10 — Observabilidade

Como o PetHotel é instrumentado para rastrear erros, performance e requisições de ponta a ponta. A observabilidade é **dividida em duas pontas que se ligam pelo `correlation id`**:

| Ponta | Ferramentas | O que cobre |
|---|---|---|
| **Backend** (`PetHotel.Api`) | Serilog (log estruturado) + OpenTelemetry (tracing + métricas via OTLP) + health checks | Requisições HTTP, traces, métricas de runtime, `/health` e `/ready` |
| **Frontend** (`frontend/`) | **Sentry** (`@sentry/react`): erros + Web Vitals + Session Replay | Exceções de UI, performance de navegação, replay de sessões com erro |

O elo entre as duas pontas é o header **`X-Correlation-Id`**: o front gera um id por requisição, manda no header (o backend o registra no log) e marca o mesmo id no escopo do Sentry. Assim um erro no Sentry pode ser cruzado com a linha de log/trace correspondente no backend.

---

## Frontend — Sentry

### Visão geral

Toda a integração mora num módulo único: **`frontend/src/shared/observability/sentry.ts`**. Ele expõe quatro funções e **tudo é "gated" pela presença da variável `VITE_SENTRY_DSN`**:

> **Sem `VITE_SENTRY_DSN` definido, `initObservability()` é no-op e nenhuma das demais funções faz nada — nenhum dado sai do navegador.**

Isso é proposital: em **dev/local** ninguém configura DSN, então o Sentry fica desligado e não há ruído nem custo. Liga-se **só em staging/produção**, onde a env é definida no build.

### Configuração (variáveis de ambiente)

As variáveis são lidas em **tempo de build** pelo Vite (prefixo `VITE_`). Defina-as no ambiente de build/deploy (ou num `.env.production` não versionado). O modelo está em `frontend/.env.example`:

| Variável | Obrigatória | Default | Para que serve |
|---|---|---|---|
| `VITE_SENTRY_DSN` | **Sim** (para ligar) | — (vazio = desligado) | DSN do projeto no Sentry. Vazio desliga toda a observabilidade. |
| `VITE_SENTRY_ENVIRONMENT` | Não | modo do build (`import.meta.env.MODE`) | Ambiente reportado nos eventos (ex.: `production`, `staging`). |
| `VITE_APP_VERSION` | Não | — | Release/versão para correlacionar eventos com o deploy. |
| `VITE_SENTRY_TRACES_SAMPLE_RATE` | Não | `0.1` | Fração de transações de tracing/Web Vitals amostradas (0..1). |
| `VITE_SENTRY_REPLAYS_SAMPLE_RATE` | Não | `0.1` | Fração de sessões normais com Session Replay (0..1). |

Notas:
- `replaysOnErrorSampleRate` é **fixo em `1.0`** no código: sessões que terminam em erro **sempre** gravam o replay, para diagnóstico.
- As taxas são saneadas (valor fora de `0..1` ou não-numérico cai no default).

Exemplo de `.env.production` (não versionar):

```env
VITE_SENTRY_DSN=https://exemplo@o0.ingest.sentry.io/0
VITE_SENTRY_ENVIRONMENT=production
VITE_APP_VERSION=2026.06.17
VITE_SENTRY_TRACES_SAMPLE_RATE=0.2
VITE_SENTRY_REPLAYS_SAMPLE_RATE=0.1
```

### O que é capturado

1. **Erros** — exceções não tratadas e erros de render (via React error boundary).
2. **Falhas de API** — respostas **5xx** e **erros de rede** (backend inacessível); 4xx ficam de fora (ver abaixo).
3. **Web Vitals / performance** — `browserTracingIntegration` instrumenta navegação e fetch, propagando `sentry-trace`/`baggage`.
4. **Session Replay** — gravação da sessão, com **privacidade por padrão** (ver abaixo).

### Falhas de API (5xx e rede)

O client da API (`src/shared/api/client.ts`) reporta ao Sentry, via `captureApiError`:

- **Respostas 5xx** (servidor com erro) — hook `onResponse`.
- **Erro de rede / backend inacessível** (o `fetch` rejeita, sem resposta HTTP) — hook `onError`.

**Falhas 4xx NÃO são capturadas** (400 validação, 401 sessão, 404, 409 conflito): são erros de negócio esperados, tratados na UI, e virariam ruído no Sentry.

> **Por isso um login com 401 (senha errada) — ou um 502 que a tela trata e exibe — pode não gerar Issue se for 4xx.** Um **5xx** ou o **backend totalmente fora** (rede) geram, sim, um evento, com tags `api_error`, `http_status`, `correlation_id` e o contexto `request` (método + pathname, **sem query string**, para não vazar termos de busca/PII). O erro continua propagando normalmente para o React Query — a captura não engole o erro nem muda o comportamento da tela.

### Privacidade / LGPD

O app lida com dados de **tutores e pets**. O Session Replay é inicializado com mascaramento agressivo para **não vazar PII**:

```ts
Sentry.replayIntegration({
  maskAllText: true,    // mascara todo texto renderizado
  maskAllInputs: true,  // mascara o conteúdo de inputs
  blockAllMedia: true,  // bloqueia imagens/vídeos (fotos de pets etc.)
})
```

Além disso, o usuário associado aos eventos carrega **apenas `id` e `tenantId`** — nunca e-mail ou nome:

```ts
Sentry.setUser({ id: claims.sub });
Sentry.setTag("tenant_id", claims.tenantId);
```

Ao customizar a captura, **não adicione PII** (e-mail, nome, telefone, documento) a tags, contexto ou breadcrumbs.

### Onde está plugado (fiação)

| Arquivo | Papel |
|---|---|
| `src/main.tsx` | Chama `initObservability()` **antes do render**, para capturar erros de carregamento. |
| `src/shared/observability/sentry.ts` | Módulo central (init + helpers). |
| `src/shared/ui/error-boundary.tsx` | `FeatureErrorBoundary.componentDidCatch` → `captureBoundaryError(err, feature, stack)` (e mantém o `console.error`). |
| `src/shared/api/client.ts` | Middleware da API: a cada request, `setCorrelationContext(X-Correlation-Id)`. |
| `src/shared/api/files.ts` | Uploads/downloads (fetch cru, fora do openapi-fetch): idem no `authHeaders()`. |
| `src/shared/auth/auth-context.tsx` | `useEffect([claims])` associa/limpa o usuário via `setObservabilityUser(...)`. |

### API do módulo

```ts
// Inicializa o Sentry. Chamar UMA vez, o mais cedo possível. No-op sem DSN.
initObservability(): void

// Marca o correlation id da request atual no escopo (tag `correlation_id`),
// para casar os eventos do front com o tracing do backend.
setCorrelationContext(correlationId: string): void

// Reporta uma exceção capturada por um error boundary, com a tag da feature.
captureBoundaryError(error: Error, feature: string, componentStack?: string | null): void

// Associa (id + tenant, sem PII) ou limpa (null) o usuário dos eventos.
setObservabilityUser(user: { id: string; tenantId?: string } | null): void
```

Para capturar um erro manualmente em algum fluxo, prefira reutilizar o boundary; se precisar reportar direto, importe o SDK no ponto de uso (`import * as Sentry from "@sentry/react"`) e use `Sentry.captureException` — lembrando que **sem DSN o SDK não envia nada**.

### Como verificar

1. Defina `VITE_SENTRY_DSN` e rode `corepack pnpm@10 run build && corepack pnpm@10 run preview`.
2. Force um erro numa tela (ou chame `Sentry.captureException(new Error("teste"))` no console).
3. Confira o evento no projeto do Sentry — deve trazer a tag `correlation_id` e, se logado, o `tenant_id`.

> **Gerência de pacotes:** o `frontend/` usa **pnpm** (store v10). Use `corepack pnpm@10 <cmd>` — `npm install` quebra contra o `node_modules` montado pelo pnpm.

---

## Backend — Serilog + OpenTelemetry + Sentry

Configurado em `src/PetHotel.Api/Program.cs` (detalhes de mensageria/resiliência em `docs/05`):

- **Serilog** — log estruturado, `ReadFrom.Configuration` (níveis em `appsettings*.json`) + `Enrich.FromLogContext` + `UseSerilogRequestLogging()` (uma linha por request HTTP).
- **OpenTelemetry** — `AddService("PetHotel.Api")`; **tracing** (`AspNetCore` + **`AddSource("Wolverine")`** p/ handlers/Outbox + **`AddSource("Npgsql")`** p/ queries + exportador OTLP) e **métricas** (`AspNetCore` + runtime + OTLP). O endpoint OTLP segue as envs padrão do OTel (`OTEL_EXPORTER_OTLP_ENDPOINT`, etc.).
- **Sentry** (`Sentry.AspNetCore`) — rastreamento de erro do backend. Captura **exceções não tratadas** da pipeline; erros de negócio usam `Result<T>` (não viram exceção), então não geram ruído. Conecta-se ao trace distribuído do front (headers `sentry-trace`/`baggage`).
- **Health checks** — `/health` (liveness: o processo está de pé, `Predicate = _ => false`) e `/ready` (readiness: executa o **`PostgresHealthCheck`** — `SELECT 1` no pool compartilhado, cobrindo banco + Outbox — via tag `ready`).

### Correlation id (o elo front ↔ backend)

O `CorrelationIdMiddleware` (registrado **antes** do request logging) lê o header **`X-Correlation-Id`** do front (ou gera um), e o propaga para as três pontas:

1. **Serilog** — `LogContext.PushProperty("CorrelationId", id)`: todo log da request carrega o id.
2. **OpenTelemetry** — `Activity.Current?.SetTag("correlation_id", id)`: vira atributo do span.
3. **Sentry** — `SentrySdk.ConfigureScope(... SetTag("correlation_id", id))`: marca o id no evento.

O id também volta no header da resposta. Assim, um erro no Sentry do front e a linha de log / trace / Issue do backend compartilham o mesmo `correlation_id`.

### Configuração do Sentry (backend)

Seção `Sentry` no `appsettings.json` (sobrescrevível por env `Sentry__*`):

| Chave | Default | Observação |
|---|---|---|
| `Sentry:Dsn` | `""` (vazio = **desligado**) | Mesmo "gate" do front. Defina por `Sentry__Dsn` em staging/prod. |
| `Sentry:SendDefaultPii` | `false` | **LGPD**: não captura PII (corpo, headers sensíveis, IP). |
| `Sentry:TracesSampleRate` | `0.1` | Amostragem de performance/tracing. |
| `Sentry:MinimumEventLevel` | `Error` | Logs `Error`+ viram eventos no Sentry. |

> **Nota (Serilog × Sentry):** a captura via `ILogger` depende do provider. Como o app usa Serilog, **logs emitidos via Serilog** que não passam pela pipeline ASP.NET não viram evento automaticamente — exceções não tratadas e o `MinimumEventLevel` cobrem o essencial. Para capturar **todos** os logs de erro estruturados, adicionar o sink `Sentry.Serilog` é uma melhoria opcional (não feita para evitar duplicar o DSN).

---

## Visualização local — Aspire Dashboard (Fase 1)

Para **ver** a telemetria OTel (traces, métricas e logs) sem montar a stack Grafana, o `docker-compose.yml` traz o **.NET Aspire Dashboard**: um container único que recebe OTLP e mostra tudo numa UI, com correlação trace ↔ logs ↔ métricas.

### Subir e usar

```bash
# 1. Sobe o dashboard (e o Postgres, se ainda não estiver de pé)
docker compose up -d aspire-dashboard postgres

# 2. Roda a API no profile de dev (já aponta o OTLP para localhost:4317)
dotnet run --project src/PetHotel.Api --launch-profile http

# 3. Gera tráfego (Swagger em http://localhost:5131/swagger) e abra a UI:
#    http://localhost:18888
```

### Portas e fluxo

| Porta (host) | Para | Observação |
|---|---|---|
| `18888` | UI do dashboard | `http://localhost:18888` (modo anônimo, sem login em dev) |
| `4317` | OTLP/gRPC | Mapeada para a `18889` do container — é o **endpoint padrão** do exportador .NET |

O profile `http`/`https` define `OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317`, então:
- **Traces/métricas** — o `AddOtlpExporter()` do OpenTelemetry exporta automaticamente.
- **Logs** — o Serilog também exporta via OTLP (sink condicional no `Program.cs`, só quando o endpoint está definido), aparecendo no dashboard correlacionados ao trace.

### O que você vê

- **Traces** — cada request com seus spans: HTTP → handler (Wolverine) → query (Npgsql). Útil para achar onde o tempo é gasto (lembrando os hotspots de banco do teste de carga).
- **Structured logs** — logs da request, com o `correlation_id` e o trace id; dá para pular de um span para os logs daquela operação.
- **Metrics** — contadores/histogramas do ASP.NET Core e do runtime (.NET GC, threads, etc.).

### Limites (é dev-only)

- **Dados em memória**: o dashboard **perde tudo ao reiniciar** o container — é para inspeção ao vivo, não histórico. Para retenção/alertas, é a Fase 2/3 (Grafana Cloud ou LGTM self-host).
- **Sem auth**: `DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true` é só para local — **nunca** expor assim.
- O profile **`loadtest` desliga o OTel** (`OTEL_SDK_DISABLED=true`) e não exporta logs por OTLP de propósito: telemetria a ~1500 req/s distorce a medição.

---

## Resumo operacional

- **Ligar o Sentry no front:** definir `VITE_SENTRY_DSN` no build. Sem isso, desligado (esperado em dev).
- **Ligar o Sentry no backend:** definir `Sentry__Dsn` (env) ou `Sentry:Dsn` (appsettings). Sem isso, desligado.
- **Ajustar volume/custo:** front `VITE_SENTRY_TRACES_SAMPLE_RATE`/`VITE_SENTRY_REPLAYS_SAMPLE_RATE`; backend `Sentry:TracesSampleRate`.
- **Privacidade:** front mascara texto/inputs e bloqueia mídia, usuário só com `id`/`tenant`; backend `SendDefaultPii=false`. Não adicionar PII ao customizar.
- **OTel:** configurar o destino OTLP por env padrão (`OTEL_EXPORTER_OTLP_ENDPOINT`); spans de HTTP, Wolverine e Npgsql já são emitidos.
- **Readiness:** `/ready` só fica `Healthy` com o Postgres acessível; use-o no orquestrador (k8s/compose) como readiness probe, e `/health` como liveness.
