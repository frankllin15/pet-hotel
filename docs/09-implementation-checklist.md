# 09 — Checklist de Implementação por Fase

Funcionalidades a implementar, em ordem de fase. Cada item é uma capacidade entregável (back + front, salvo indicação). A ordem respeita a priorização: **Reservas → Saúde → Operação → Financeiro**, com hotelagem antes de creche/banho&tosa (reaproveitam cadastro, calendário e clearance).

Legenda: `[B]` backend · `[F]` frontend · sem marca = ambos.

---

## Fundação (pré-fases — habilita tudo)

- [x] Solução .NET 10 + estrutura modular hexagonal (`docs/01`)
- [x] `SharedKernel`: Entity, AggregateRoot, ValueObject, IDomainEvent, Result, Error, Ids tipados `[B]`
- [x] `BuildingBlocks`: ITenantContext, interceptor de tenant + auditoria `[B]`
- [x] Wolverine + EF Core + Outbox no PostgreSQL (`docs/05`) `[B]`
- [x] Schema do Wolverine isolado (ex.: `wolverine`) `[B]`
- [x] Testes de arquitetura (NetArchTest) travando a regra de dependência `[B]`
- [ ] CI/CD com aplicação de migrations via bundle/migrator (não no startup) `[B]` — _migrations aplicadas manualmente em dev; pipeline ainda não montado_
- [x] App Vite + React + TS, shell, router, providers, TanStack Query (`docs/08`) `[F]`
- [x] Geração do client TS a partir do OpenAPI `[F]`
- [x] Design tokens, `AsyncBoundary`, arquétipos de tela base, shadcn/ui `[F]`
- [ ] Observabilidade: Serilog + OpenTelemetry + correlation id; Sentry no front — _Serilog+OTel+correlation id no back e `X-Correlation-Id` no front prontos; **Sentry no front pendente**_
- [x] Health checks `/health` (liveness) e `/ready` (readiness) `[B]`

---

## Fase 1 — Núcleo navegável

Entrega o ciclo "cadastrar → validar saúde → reservar → check-in/out" para hotelagem.

### Tenancy & Acesso
- [x] Provisionamento de tenant (Tenant + admin + TenantConfiguration default, atômico)
- [x] Autenticação (Identity/IdP) com claims de tenant e papel (RBAC)
- [x] Convite de usuário (token único, expiração) + definição de senha no 1º acesso — _**e-mail real pendente**: token volta no corpo (MVP)_
- [x] Wizard de onboarding (tipos de acomodação, preços, regras de vacina, horários)
- [x] Guards de rota por papel `[F]`

### Registry (Cadastros)
- [x] Cadastro de tutor (dados, faturamento, contatos de emergência, autorizados a retirar) — _faturamento (CPF/CNPJ, e-mail de cobrança, endereço) como JSON no agregado_
- [x] Múltiplos pets por tutor
- [ ] Cadastro de pet (raça, idade, porte, sexo, castração, foto, microchip) — _parcial: nome/espécie/raça/nascimento/notas, **porte, sexo, castração e microchip** prontos; **foto** pendente_
- [x] Avaliação comportamental (sociabilidade, reatividade, medo, destrutividade) — _4 níveis (Baixa/Média/Alta) + notas, editável na ficha; base da gestão de matilhas_
- [x] Rotina alimentar (ração, quantidade, horários, restrições, origem da ração) — _JSON no agregado Pet; editável no cadastro e na ficha_
- [ ] Pertences trazidos (checklist de conferência)
- [ ] Estado de chegada com foto; termo/consentimento LGPD

### Health (Saúde)
- [ ] Carteira de vacinação com upload de foto e validade — _parcial: tipo/aplicação/validade prontos; **upload de foto pendente**_
- [x] Contrato público de *clearance* (vacina em dia?) consumível pelo Booking
- [x] Controle de parasitas (antipulgas/vermífugo) — _tipo/produto/aplicação/próxima dose; situação em dia/vencido/sem previsão_
- [x] Contato do veterinário particular — _na ficha de saúde (JSON), editável na aba Saúde_

### Booking (Reservas — hotelagem)
- [ ] Tipos de acomodação configuráveis por tenant — _parcial: `AccommodationType` no wizard + CRUD de acomodações; vínculo tipo→acomodação a amadurecer_
- [x] Calendário visual de ocupação (com virtualização) `[F]` — _calendário pronto (estilo Google Agenda); **virtualização pendente** (docs/08 só exige em escala)_
- [ ] Criar reserva (pet, período, acomodação, serviços adicionais) — _parcial: pet/período/acomodação prontos; serviços adicionais pendentes_
- [x] **Bloqueio/alerta por vacina vencida** no fluxo de reserva
- [x] Concorrência otimista (xmin) contra overbooking `[B]`
- [ ] Gestão de matilhas (compatibilidade comportamental) — critério objetivo a definir
- [ ] Precificação por porte/necessidade/feriado/alta temporada
- [x] Check-in / check-out — _ciclo `Confirmed→CheckedIn→CheckedOut` com horários reais carimbados; **regra de diária/late-checkout pendente**_

---

## Fase 2 — Operação e Comunicação

### Operations (Diário de bordo)
- [ ] Checklist de tarefas (limpeza, alimentação, recreação)
- [ ] Registro rápido de ocorrências (comeu, fezes, comportamento)
- [ ] Timeline por pet com fotos/vídeos (com virtualização) `[F]`
- [ ] Atribuição de responsável / escala do dia
- [ ] Log de administração de medicamento (quem, quando, dose)
- [ ] Fluxo de incidente grave (auditável + notificação ao tutor)

### Notifications (Comunicação)
- [ ] Montagem de relatório diário (texto + mídia) a partir do diário
- [ ] Envio exportável/compartilhável (sem WhatsApp ainda)
- [ ] Histórico de envios por tutor

### Dashboards
- [ ] Painel do dia consolidado (chegadas, saídas, medicações)
- [ ] Alertas consolidados (vacinas vencendo, medicações, estoque baixo)
- [ ] Taxa de ocupação por período/acomodação

---

## Fase 3 — Monetização e Serviços Completos

### Billing (Financeiro)
- [ ] Controle de pagamentos e recibos
- [ ] Pacotes (créditos consumíveis por diária)
- [ ] Assinaturas/planos mensais recorrentes
- [ ] Formas de pagamento (PIX, recorrência) — via Outbox/idempotência
- [ ] Fechamento financeiro por período

### Grooming (Banho & Tosa)
- [ ] Agenda de profissionais + tempo por serviço
- [ ] Ficha de tosa (tipo, ferramenta, observações, fotos antes/depois)
- [ ] Vínculo a pet hospedado (banho na saída) ou avulso
- [ ] Comissão por profissional

### Daycare (Creche)
- [ ] Check-in/out diário (não por pernoite)
- [ ] Planos mensais com controle de frequência
- [ ] Consumo de créditos por dia frequentado

### Inventory (Estoque)
- [ ] Cadastro de produtos (limpeza, ração, petiscos, medicamentos)
- [ ] Baixa por consumo + alerta de mínimo

### Comunicação
- [ ] Integração com WhatsApp (BSP) com resiliência (Polly) e Outbox

---

## Fase 4 — Opcionais

- [ ] Transporte / Táxi Dog (agendamento, rota, cobrança)
- [ ] Portal do tutor (app separado — reavaliar Next.js)
- [ ] Acesso de login para veterinário externo
- [ ] Tema por tenant (branding) via tokens
- [ ] SignalR para alertas em tempo real (medicação)
- [ ] Banco dedicado para tenants grandes
- [ ] Storybook + regressão visual

---

## Critérios de "pronto" por fase

- **Fase 1:** fluxo E2E coberto por teste — cadastrar → vacina → reservar → confirmar bloqueado por vacina vencida → confirmar liberado → ver no calendário.
- **Fase 2:** um dia de operação real registrável de ponta a ponta, com relatório enviável ao tutor.
- **Fase 3:** uma estadia faturável com pacote/assinatura, banho e creche operando sobre o mesmo cadastro.
- **Fase 4:** conforme demanda — nenhum item é pré-requisito do MVP.
