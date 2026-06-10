# 08 — Frontend (Instruções de Implementação)

App **interno de operação**, multi-tenant, usado em dispositivos mistos: desktop denso (recepção/gerência) e mobile/tablet (tratadores, banho/tosa), com possível conexão instável no canil. Não é site público — as escolhas otimizam para ferramenta de uso diário atrás de login.

## Decisões travadas

- **Framework:** Vite + React + TypeScript (SPA). *Recomendação — sobrescrevível.* Next.js fica reservado para um futuro portal do tutor (app separado), não para a operação interna.
- **Estado de servidor:** TanStack Query. **Estado de UI:** Zustand (leve) — sem misturar os dois.
- **UI base:** Tailwind + shadcn/ui (Radix). *Recomendação — sobrescrevível* (alternativa: Chakra).
- **Formulários:** react-hook-form + zod.
- **Client de API:** gerado a partir do OpenAPI do backend (type-safe).
- **Tipos/identificadores:** inglês. Comentários/doc: pt-BR.

## Estrutura do projeto

Organização **por feature, espelhando os bounded contexts do backend** — não por tipo técnico.

```
src/
├── app/                      # shell, providers, router, layout raiz
├── shared/
│   ├── ui/                   # componentes primitivos (shadcn) + de domínio
│   ├── api/                  # client gerado (OpenAPI) + setup do Query
│   ├── lib/                  # utils, AsyncBoundary, error mapping
│   └── auth/                 # contexto de auth, guards por papel (RBAC)
├── features/
│   ├── registry/             # tutores e pets
│   ├── health/               # vacinas, medicações
│   ├── booking/              # reservas, calendário de ocupação
│   ├── operations/           # diário de bordo, checklist
│   ├── grooming/ daycare/ billing/ inventory/ tenancy/
│   └── <feature>/
│       ├── components/       # componentes só desta feature
│       ├── hooks/            # useQuery/useMutation desta feature
│       ├── pages/            # telas, montadas sobre os arquétipos
│       └── schemas.ts        # zod desta feature
└── routes/                   # definição de rotas + guards
```

## Type safety com o backend

- Gerar o client TS a partir do OpenAPI do .NET (openapi-typescript, orval ou Kiota) num passo de build/script.
- Request/response e endpoints vêm do contrato; mudança de DTO no backend **quebra o build** do front. É o "teste de arquitetura" do frontend.
- Não digitar tipos de API à mão.

## Estado

- **Servidor → TanStack Query.** Cache, retry com backoff, refetch em background, dedupe e atualização otimista saem daqui.
- **UI puro → Zustand/context** (filtros abertos, tema, passo de wizard). Nunca colocar dado de servidor em store global.
- Chaves de query padronizadas por feature (`['booking','reservations',params]`).

## Sistema de UI (consistência de telas)

Consistência vem de **reduzir decisões por tela**, em camadas:

### 1. Design tokens
Cor, espaçamento, tipografia, raio, sombra, breakpoints no theme do Tailwind. **Proibido valor mágico** (`mt-[13px]`, `#3b82f6`) — só token. Tokens via CSS variable também habilitam tema por tenant no futuro, sem tocar em componente.

### 2. Componentes em três níveis
- **Primitivos:** shadcn/Radix (Button, Input, Dialog) — não reinventar.
- **De domínio:** `PetCard`, `ReservationStatusBadge`, `VaccineStatusTag`, `OccupancyCell` — construídos uma vez, reusados.
- **Padrões compostos:** cabeçalho de página com ações, barra de filtros, painel de detalhe.

### 3. Arquétipos de tela *(o que mais garante consistência)*
Toda tela nova encaixa num arquétipo existente, não inventa layout:

| Arquétipo | Anatomia | Telas do MVP |
|---|---|---|
| **Lista/Tabela** | filtro fixo no topo, tabela, paginação por cursor, ação primária no mesmo canto | reservas, pets, tutores, produtos |
| **Detalhe** | cabeçalho + abas + painel lateral | ficha do pet, ficha da reserva |
| **Form criar/editar** | mesma estrutura de campos, validação e botões | cadastro tutor/pet, nova reserva |
| **Dashboard** | grid de cards com anatomia única | painel da gerência |
| **Calendário** | grade de ocupação (caso especial, único) | ocupação |

### 4. Estados padronizados
Wrapper único sobre o resultado do Query, com loading/erro/vazio/sucesso iguais em todo lugar. Ninguém escreve seu próprio spinner.

```tsx
<AsyncBoundary query={reservationsQuery} empty={<NoReservations />}>
  {(data) => <ReservationsTable rows={data} />}
</AsyncBoundary>
```

### 5. Ações com consequência *(confirmação didática)*
Toda ação que **muda estado de forma relevante ou irreversível** (transições de máquina de estado, exclusões, envios ao mundo externo) **não dispara direto no clique** — passa pelo primitivo único `ConfirmDialog`, com uma mensagem que **explica o efeito antes de o usuário confirmar**. Regras:

- **Contexto, não genérico:** a mensagem nomeia os dados concretos da ação (pet, acomodação, período), não um texto abstrato tipo "Tem certeza?".
- **Consequência explícita:** dizer o que muda e, principalmente, o que **não dá para desfazer** ou o que fica **bloqueado** depois (ex.: "após o check-in a reserva não pode mais ser cancelada"; "confirmar valida a aptidão sanitária e bloqueia se a vacina estiver vencida").
- **Antecipa o erro do backend:** quando uma invariante do domínio pode barrar a ação (`docs/03`), o aviso a explica *antes*, em vez de só mostrar o 409 depois.
- **Visual casa com o risco:** ação destrutiva usa `confirmVariant="destructive"`; rótulos sem ambiguidade ("Cancelar reserva" vs. "Voltar").
- **Reúso:** texto por ação centralizado (mapa de metadados + um componente de conteúdo por feature), nunca espalhado em cada botão.

Referência: `features/booking/pages/reservations-page.tsx` (`ACTION_META` + `ReservationActionInfo`) sobre `shared/ui/confirm-dialog.tsx`. Leituras e ações triviais/reversíveis (filtrar, navegar, abrir form) **não** pedem confirmação — o atrito é reservado para o que importa.

## Performance

- Code-splitting por rota/feature (o tratador não baixa o módulo financeiro).
- **Virtualização** (TanStack Virtual) no calendário de ocupação e na timeline do diário de bordo.
- Paginação por **cursor/keyset** alinhada ao backend (`docs/04`).
- **Atualização otimista** nos fluxos rápidos (check-in/out de creche, marcar tarefa), com rollback no erro.
- Memoização só onde o profiler apontar.

## Confiabilidade

- **Conexão instável é requisito:** cache do Query servindo dados ao reconectar, retry automático, estados de erro com "tentar de novo" explícito.
- **Error boundaries por feature** — tela quebrada não derruba o app.
- Loading/erro/vazio como estados de primeira classe em toda tela.
- **Idempotency key** em mutações com efeito externo (cobrança, mensagem ao tutor), casando com o backend (`docs/05`).

## Consistência com o backend (erros e validação)

- Camada única que traduz `ProblemDetails`/`Result` do backend:
  - `Validation` → erro no campo do formulário
  - `Conflict` (vacina vencida, overbooking) → aviso na tela
  - `Unexpected` → toast genérico + log
- Validação com **zod** é UX; o backend é a fonte da verdade.

## Auth, multi-tenancy e onboarding

- Token traz **claims de tenant e papel (RBAC)**; rotas e ações se adaptam ao papel (tratador não vê financeiro).
- **Sem auto-cadastro aberto.** Usuários entram por **convite** (token único, com expiração); definem senha no primeiro acesso.
- **Onboarding do tenant:** após o primeiro login do admin, um **wizard de setup** coleta o mínimo para operar (tipos de acomodação, preços, regras de vacina, horários). Bloquear operação normal até o mínimo existir.
- Não implementar armazenamento de senha no front; fluxo de auth conversa com o backend (Identity/IdP).

## Frescor dos dados

- Telas que precisam estar atualizadas (ocupação, alertas de medicação, vacina vencendo, painel do dia): **polling** via `refetchInterval` do Query no MVP.
- Migrar para **SignalR** (push) quando o alerta de medicação precisar de precisão. Não começar com WebSocket.

## Testes e observabilidade

- Testing Library (componentes), **Playwright** para fluxos críticos, **MSW** para mockar a API.
- Fluxo crítico de E2E (espelha o backend): cadastrar → vacina → reservar → confirmar bloqueado por vacina vencida → confirmar liberado → ver no calendário.
- Produção: rastreamento de erro (Sentry) + Web Vitals; propagar **correlation id** para casar com o tracing do backend.

## Enforcement (não deixar a consistência decair)

- **Lint** bloqueando valores arbitrários / forçando token — falha mecânica, não revisão manual.
- **Este documento** como catálogo para o Claude Code: toda tela nova encaixa num arquétipo; todo dado usa `AsyncBoundary`; toda ação com consequência usa `ConfirmDialog` com mensagem didática (§5).
- **Storybook** + regressão visual: introduzir quando a biblioteca de componentes estabilizar, não no dia 1.

## Ordem de implementação

1. **Fundação:** shell do app, router, providers, setup do Query, client OpenAPI, auth + guards, design tokens, `AsyncBoundary`, arquétipos de tela base.
2. **Feature `tenancy`/auth:** login, aceite de convite, wizard de onboarding.
3. **`registry`:** listas e formulários de tutor/pet (exercita os arquétipos Lista e Form).
4. **`health`:** vacinas/medicações.
5. **`booking`:** calendário de ocupação + fluxo de reserva (exercita o arquétipo Calendário e a integração de erro de "vacina vencida").

Demais features seguem o roadmap em `docs/09-implementation-checklist.md`.
