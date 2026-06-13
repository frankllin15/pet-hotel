using PetHotel.SharedKernel;

namespace PetHotel.Operations.Domain.CareLog.Events;

/// <summary>Uma ocorrência foi registrada no diário (futuro consumo: relatório do Notifications via Outbox).</summary>
public sealed record CareEntryLogged(CareLogEntryId EntryId, TenantId TenantId, PetReference Pet) : IDomainEvent;
