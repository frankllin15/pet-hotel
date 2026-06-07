# 05 — Mensageria e Confiabilidade (Wolverine)

Wolverine cobre dois papéis: **mediator in-process** (despacho de Commands/Queries para handlers) e **mensageria durável** (Outbox/Inbox). Core é MIT (open-core); a monetização da JasperFx vem de suporte e do CritterWatch (monitoramento) — a biblioteca em si é gratuita.

> As APIs do Wolverine evoluem entre versões maiores (4.x atual). Confirme nomes exatos de método contra a doc da versão instalada — este arquivo descreve o **padrão**, não a assinatura exata.

## Pacotes

- `WolverineFx` — core (handlers + mediator).
- `WolverineFx.Http` — endpoints HTTP (opcional, alternativa ao Minimal API).
- `WolverineFx.EntityFrameworkCore` — transações/Outbox integrados ao `DbContext`.
- `WolverineFx.Postgresql` — armazenamento durável de mensagens (envelope storage) em PostgreSQL.

## Configuração (padrão, no host)

```csharp
builder.Host.UseWolverine(opts =>
{
    // armazenamento durável de mensagens no Postgres (Outbox/Inbox)
    opts.PersistMessagesWithPostgresql(connectionString);

    // transações via EF Core + Outbox automático nos handlers
    opts.UseEntityFrameworkCoreTransactions();
    opts.Policies.AutoApplyTransactions();

    // garantia de processamento durável local (sem broker externo no MVP)
    opts.Policies.UseDurableLocalQueues();
});
```

No MVP **não há broker**: eventos de integração trafegam em filas locais duráveis persistidas no Postgres. Quando o volume exigir, troca-se para um transport (RabbitMQ/Service Bus) sem reescrever handlers.

## Mediator (Commands/Queries)

```csharp
// na API
var result = await _bus.InvokeAsync<Result<ReservationId>>(command, ct);
```

Handler é descoberto por convenção (classe `*Handler` com método `Handle`). Handler fino: orquestra, devolve `Result`.

## Domain Events vs Integration Events

- **Domain Event:** interno ao agregado, despachado **após** o `SaveChanges`. Reage dentro do mesmo módulo/transação lógica.
- **Integration Event:** cruza módulo ou vai ao mundo externo. **Sempre via Outbox**, gravado na **mesma transação** do agregado.

Fluxo: agregado levanta `ReservationConfirmed` (domain event) → após commit, um handler publica `ReservationConfirmedIntegrationEvent` no Outbox → `Notifications` e `Billing` consomem em suas próprias transações.

## Outbox (regra inegociável)

Tudo que sai do módulo ou vai para fora (WhatsApp, pagamento) passa pelo Outbox:
- A mensagem é persistida **atomicamente** com a mudança do agregado (mesma transação, garantido pelo `AutoApplyTransactions`).
- Um worker durável despacha; **consumidores são idempotentes**.
- **ACK só após persistência** em qualquer receptor — nunca confirmar antes de gravar.

## Idempotência

- Comandos com efeito externo (cobrança, envio de mensagem) carregam **idempotency key**.
- Consumidores deduplicam por chave → efeito "processa-uma-vez".

## Resiliência (chamadas externas)

- `Microsoft.Extensions.Http.Resilience` (Polly) nos gateways de WhatsApp/pagamento: timeout, retry com backoff, circuit breaker, fallback.
- Mensagens do Outbox que esgotam retries vão para **dead-letter** com visibilidade para operação.

## Observabilidade

- **OpenTelemetry**: tracing distribuído + métricas. Wolverine emite spans dos handlers.
- **Serilog** com logs estruturados e **correlation id** propagado desde a borda da API.
- **Health checks**: separar *liveness* (`/health`) de *readiness* (`/ready`, que checa banco e Outbox).

## Background work

- Despacho do Outbox, geração de relatórios e envio de mídia ao tutor rodam em workers — picos de check-in/out de manhã/fim de tarde não bloqueiam a request.
- API **stateless**: nenhum estado em memória entre requests; lock/contador/sessão vão para Redis quando necessário.
