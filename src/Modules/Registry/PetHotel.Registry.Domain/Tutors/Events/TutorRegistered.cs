using PetHotel.SharedKernel;

namespace PetHotel.Registry.Domain.Tutors.Events;

/// <summary>Um novo tutor foi cadastrado em um tenant.</summary>
public sealed record TutorRegistered(TutorId TutorId, TenantId TenantId, string Email) : IDomainEvent;
