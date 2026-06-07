# 01 — Arquitetura

Hexagonal (Ports & Adapters) + DDD + Monólito Modular. Um deploy, módulos isolados por bounded context, com costuras para extrair um módulo como serviço no futuro sem reescrita.

## Regra de dependência (testada em `docs/06`)

```
API (driving adapter)
        │ depende de
        ▼
Application ──define──► Ports (interfaces de saída)
        │ depende de
        ▼
Domain (não depende de nada externo)
        ▲
        │ implementa as portas
Infrastructure (driven adapters: EF, Wolverine, gateways externos)
```

- `Domain`: sem referência a EF, ASP.NET, Wolverine ou qualquer pacote de infra.
- `Application`: referencia `Domain`; **declara** as portas de saída (repositórios, gateways).
- `Infrastructure`: referencia `Application`/`Domain`; **implementa** as portas.
- `API`: orquestra, não contém regra de negócio.

## Estrutura da solução

```
PetHotel.sln
├── src/
│   ├── PetHotel.Api/                        # Host: Wolverine.HTTP/Minimal API, DI, middleware
│   ├── BuildingBlocks/
│   │   ├── PetHotel.SharedKernel/           # Entity, AggregateRoot, ValueObject, IDomainEvent, Result<T>, Error
│   │   └── PetHotel.BuildingBlocks/         # Outbox, Tenancy, Auditing, interceptors EF, resiliência
│   └── Modules/
│       ├── Tenancy/        { Domain, Application, Infrastructure }
│       ├── Registry/       { Domain, Application, Infrastructure }
│       ├── Health/         { Domain, Application, Infrastructure }
│       ├── Booking/        { Domain, Application, Infrastructure }
│       ├── Grooming/       { ... }
│       ├── Daycare/        { ... }
│       ├── Operations/     { ... }
│       ├── Notifications/  { ... }
│       ├── Billing/        { ... }
│       └── Inventory/      { ... }
└── tests/
    ├── PetHotel.ArchitectureTests/
    ├── PetHotel.<Module>.UnitTests/
    └── PetHotel.IntegrationTests/
```

## Anatomia de um módulo (ex.: `Booking`)

```
Booking/
├── PetHotel.Booking.Domain/
│   ├── Reservations/
│   │   ├── Reservation.cs            # Aggregate Root
│   │   ├── ReservationStatus.cs      # Value Object
│   │   ├── DateRange.cs              # Value Object
│   │   └── Events/ReservationConfirmed.cs
│   ├── Accommodations/Accommodation.cs
│   └── Ports/IReservationRepository.cs       # porta de saída
│
├── PetHotel.Booking.Application/
│   ├── Reservations/
│   │   ├── Commands/CreateReservation/        # Command + Handler + Validator
│   │   └── Queries/GetOccupancyCalendar/
│   ├── Abstractions/IHealthClearanceGateway.cs  # porta p/ consultar outro módulo
│   └── Contracts/                              # contratos PÚBLICOS consumíveis por outros módulos
│
└── PetHotel.Booking.Infrastructure/
    ├── Persistence/
    │   ├── BookingDbContext.cs
    │   ├── Configurations/                     # IEntityTypeConfiguration<>
    │   ├── Repositories/
    │   └── Migrations/
    └── Adapters/                               # implementações de gateways externos
```

## Comunicação entre módulos

Proibido: um módulo referenciar entidades, DbContext ou tabelas de outro.

Permitido, somente:
1. **Contratos públicos** — interfaces/DTOs expostos em `<Module>.Application/Contracts`, implementados pelo módulo dono.
2. **Eventos de integração** — publicados via Outbox e consumidos por handlers de outro módulo.

Exemplo: ao criar reserva, `Booking` precisa saber se a vacina está em dia. Ele **não** lê tabelas de `Health` — chama `IHealthClearanceGateway` (porta na Application do Booking), cujo adaptador invoca um contrato público do módulo `Health`.

## Por que monólito modular

O volume de um hotel de pets não justifica microsserviços. Os bounded contexts dão isolamento lógico; a separação física fica como evolução. Quando (e se) um módulo virar gargalo, ele já está desacoplado por contrato/evento e pode sair como serviço sem tocar nos demais.
