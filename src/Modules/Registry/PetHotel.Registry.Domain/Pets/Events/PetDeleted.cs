using PetHotel.Registry.Domain.Tutors;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Domain.Pets.Events;

/// <summary>
/// Um pet foi excluído. Consumível (futuro, via Outbox) por outros módulos para limpeza
/// — ex.: Health remove a ficha/fotos, Booking trata reservas órfãs (docs/05).
/// </summary>
public sealed record PetDeleted(PetId PetId, TenantId TenantId, TutorId TutorId) : IDomainEvent;
