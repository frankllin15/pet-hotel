using PetHotel.SharedKernel;

namespace PetHotel.Registry.Domain.Packs.Events;

/// <summary>Matilha excluída (despacho via Outbox é futuro).</summary>
public sealed record PackDeleted(PackId PackId, TenantId TenantId) : IDomainEvent;
