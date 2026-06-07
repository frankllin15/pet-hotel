# 03 — Modelagem de Domínio (DDD)

## Princípios táticos

- **Agregado = fronteira de transação e de consistência.** Tudo dentro de um agregado é consistente imediatamente; entre agregados, consistência é eventual (via eventos/Outbox).
- **Mantenha agregados pequenos.** Referência a outro agregado é **por Id** (`PetId`, `TenantId`), nunca por navegação direta.
- **Value Objects** para conceitos sem identidade própria, sempre imutáveis e auto-validados.
- **Domain Events** registram fatos já ocorridos dentro do agregado; despachados **após** o commit.
- **Specifications** encapsulam regras de consulta reutilizáveis (ex.: acomodações compatíveis com um perfil).

## Tipos base (SharedKernel)

```csharp
public abstract class Entity<TId> { public TId Id { get; protected set; } }

public abstract class AggregateRoot<TId> : Entity<TId>
{
    private readonly List<IDomainEvent> _events = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _events;
    protected void Raise(IDomainEvent e) => _events.Add(e);
    public void ClearEvents() => _events.Clear();
}

public abstract record ValueObject;             // igualdade estrutural via record
public interface IDomainEvent;                  // marker
```

Identificadores tipados (evita trocar Ids por engano): `public readonly record struct ReservationId(Guid Value);`

## Exemplo de agregado (resumido)

```csharp
public sealed class Reservation : AggregateRoot<ReservationId>
{
    public TenantId TenantId { get; private set; }
    public PetId PetId { get; private set; }            // referência por Id
    public AccommodationId AccommodationId { get; private set; }
    public DateRange Period { get; private set; }
    public ReservationStatus Status { get; private set; }

    private Reservation() { }                            // EF

    public static Result<Reservation> Create(/* ... */)
    {
        // valida invariantes; em sucesso, Raise(new ReservationRequested(...))
    }

    public Result Confirm(HealthClearance clearance)
    {
        if (!clearance.IsValid) return Error.Conflict("vaccine.expired", "Vacina vencida");
        if (Status != ReservationStatus.Requested)
            return Error.Conflict("reservation.invalid_state", "Estado inválido");
        Status = ReservationStatus.Confirmed;
        Raise(new ReservationConfirmed(Id, TenantId, PetId));
        return Result.Success();
    }
}
```

Regra-chave do negócio ("vacina vencida bloqueia reserva") é uma **invariante no agregado**, não validação solta.

## Bounded contexts e agregados

| Módulo | Agregados | Notas |
|---|---|---|
| **Tenancy** | `Tenant`, `User`, `TenantConfiguration` | RBAC, config por hotel (tipos de acomodação, preços, regras de vacina). |
| **Registry** | `Tutor`, `Pet` | `Pet` guarda perfil comportamental, rotina alimentar, pertences. |
| **Health** | `HealthRecord` | Vacinas (com validade), parasitas, medicações; expõe contrato de "clearance". |
| **Booking** | `Reservation`, `Accommodation` | Concorrência otimista crítica (overbooking). |
| **Grooming** | `GroomingAppointment` | Ficha de tosa, comissão. |
| **Daycare** | `DaycareEnrollment`, `DailyCheckIn` | Frequência diária / plano mensal. |
| **Operations** | `CareLog`, `Incident` | Diário de bordo, incidente grave auditável. |
| **Notifications** | `OutboundMessage` | Mensagem ao tutor (relatório, mídia). |
| **Billing** | `Invoice`, `Package`, `Subscription`, `Payment` | Pacotes/créditos, assinaturas. |
| **Inventory** | `Product`, `StockMovement` | Estoque, alerta de mínimo. |

## Consistência entre agregados

Não orquestrar dois agregados na mesma transação por conveniência. Padrão:
1. Operação confirma agregado A e levanta evento de domínio.
2. Após commit, o evento é despachado.
3. Se a reação cruza módulo/sistema externo, vira **evento de integração via Outbox** (ver `docs/05`).

Exemplo: `ReservationConfirmed` → módulo `Notifications` agenda relatório; módulo `Billing` cria/atualiza fatura. Cada um em sua própria transação.

## Ordem de construção (MVP)

1. **Tenancy** — sem ele nada é isolado.
2. **Registry** — Tutor/Pet, base de tudo.
3. **Health** — vacinas + contrato de clearance.
4. **Booking** — reserva consumindo o clearance de Health.

Só depois: Grooming, Daycare, Operations, Notifications, Billing, Inventory. Hotelagem primeiro porque os demais reaproveitam cadastro, calendário e clearance.
