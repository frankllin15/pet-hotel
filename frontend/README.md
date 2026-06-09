# PetHotel — Frontend

App interno de operação (multi-tenant, atrás de login). Stack e decisões em
`../docs/08-frontend.md`. Roadmap em `../docs/09-implementation-checklist.md`.

## Stack

- **Vite + React + TypeScript** (SPA)
- **TanStack Query** (estado de servidor) · **Zustand** (estado de UI)
- **Tailwind v4 + shadcn/ui** (Radix) — tokens em `src/index.css`
- **react-hook-form + zod** (formulários)
- **openapi-fetch + openapi-typescript** (client type-safe gerado do OpenAPI)

## Comandos

```bash
pnpm dev          # dev server (proxy /v1 e /openapi -> backend :5131)
pnpm build        # type-check + build de produção
pnpm typecheck    # só type-check
pnpm lint         # eslint
pnpm gen:api      # gera src/shared/api/schema.d.ts do OpenAPI (backend rodando)
```

> Antes de usar o client tipado, suba o backend (perfil `http`, porta 5131) e
> rode `pnpm gen:api`. Sem isso, `schema.d.ts` é só um placeholder.

## Estrutura

Por feature, espelhando os bounded contexts do backend (não por tipo técnico):

```
src/
├── app/        # providers, router, layout raiz (shell)
├── routes/     # árvore de rotas + guards
├── shared/
│   ├── ui/         # primitivos (shadcn), AsyncBoundary, archetypes de tela
│   ├── api/        # client gerado (OpenAPI) + QueryClient
│   ├── lib/        # utils, correlation id, mapeamento de ProblemDetails
│   ├── auth/       # contexto de auth, claims, guards por papel (RBAC)
│   └── hooks/
└── features/   # registry, health, booking, ... (components/hooks/pages/schemas)
```

## Convenções (enforce — docs/08)

- **Toda tela encaixa num arquétipo** (`shared/ui/archetypes`): Lista, Detalhe,
  Form, Dashboard. Nenhuma tela inventa layout.
- **Todo dado de servidor passa por `<AsyncBoundary>`** — loading/erro/vazio
  padronizados. Ninguém escreve seu próprio spinner.
- **Proibido valor mágico de estilo** (`mt-[13px]`, `#3b82f6`): só design token.
- **Tipos de API nunca são digitados à mão** — vêm do `gen:api`.
- Erros do backend traduzidos em uma camada só (`shared/lib/problem-details.ts`).
