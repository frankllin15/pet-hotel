# CLAUDE.md — Hotel de Pets (Backend)

Documento raiz lido automaticamente pelo Claude Code. Contém as **decisões travadas** e as **regras inegociáveis**. Detalhes por tema estão em `docs/` — consulte o arquivo específico antes de trabalhar em cada área.

## Stack (travada)

- **.NET 10** (LTS)
- **EF Core** (provider **Npgsql** / PostgreSQL) + **EF Migrations**
- **PostgreSQL** como banco
- **Wolverine** (JasperFx) como mediator in-process **e** mensageria/outbox — core é MIT/open-core
- **DDD** (tático + estratégico) + **Arquitetura Hexagonal** (Ports & Adapters)
- **Monólito Modular** (um deploy, módulos isolados por bounded context)

## Decisões de arquitetura travadas

1. **Multi-tenancy:** banco compartilhado + discriminador `TenantId` (row-level) com Global Query Filter. Tenants grandes podem ser promovidos a banco dedicado depois, sem mudar o domínio.
2. **Mediator/mensageria:** Wolverine para comandos in-process e para o Outbox.
3. **Mensageria inicial:** in-process via Outbox (sem broker externo no MVP).
4. **Banco:** PostgreSQL. Concorrência otimista via `xmin` (sem coluna extra).

## Regras inegociáveis

- **Regra de dependência:** `Domain` não conhece EF, ASP.NET ou infraestrutura. `Application` define as portas; `Infrastructure` as implementa. `API` é só adaptador de entrada. → `docs/01-architecture.md`
- **Agregado = fronteira de transação.** Referência a outro agregado é **por Id**, nunca por navegação. → `docs/03-domain-modeling.md`
- **Módulos não se acoplam:** comunicação só por contratos públicos ou eventos (Outbox). Um módulo nunca lê tabela de outro. → `docs/01-architecture.md`
- **Falhas esperadas usam `Result<T>`**, não exceções. → `docs/02-coding-conventions.md`
- **Toda escrita carimba `TenantId`** automaticamente (interceptor); toda leitura tem o query filter de tenant. → `docs/04-persistence.md`
- **O que sai do módulo ou vai ao mundo externo passa pelo Outbox** (atômico com a transação). → `docs/05-messaging-reliability.md`
- **Não rodar migrations no startup em produção.** → `docs/04-persistence.md`

## Índice da documentação

| Arquivo | Quando consultar |
|---|---|
| `docs/01-architecture.md` | Estrutura da solução, camadas, regra de dependência, comunicação entre módulos. |
| `docs/02-coding-conventions.md` | Padrões de código: Result, validação, naming, DI, endpoints, erros. |
| `docs/03-domain-modeling.md` | Agregados, value objects, eventos de domínio, bounded contexts. |
| `docs/04-persistence.md` | EF Core + PostgreSQL, multi-tenancy, migrations, concorrência, UoW. |
| `docs/05-messaging-reliability.md` | Wolverine, Outbox, eventos, resiliência, idempotência, observabilidade. |
| `docs/06-testing.md` | Estratégia de testes: unit, integração (Testcontainers), arquitetura. |
| `docs/07-getting-started.md` | Passo a passo para criar a solução e os primeiros módulos. |

## Módulos (bounded contexts)

`Tenancy` · `Registry` (cadastros) · `Health` · `Booking` · `Grooming` · `Daycare` · `Operations` · `Notifications` · `Billing` · `Inventory`

Ordem de construção do MVP: **Tenancy → Registry → Health → Booking** primeiro (núcleo navegável). Demais módulos depois. Detalhe em `docs/03-domain-modeling.md`.

## Idioma

- Código, identificadores e nomes de arquivo: **inglês**.
- Comentários e documentação: **português (pt-BR)**.
