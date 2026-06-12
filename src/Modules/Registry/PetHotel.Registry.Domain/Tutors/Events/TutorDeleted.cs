using PetHotel.SharedKernel;

namespace PetHotel.Registry.Domain.Tutors.Events;

/// <summary>Um tutor foi excluído (só é permitido quando não há pets vinculados).</summary>
public sealed record TutorDeleted(TutorId TutorId, TenantId TenantId) : IDomainEvent;
