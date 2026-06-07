using PetHotel.Registry.Domain.Tutors;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Domain.Pets.Events;

/// <summary>Um novo pet foi cadastrado para um tutor.</summary>
public sealed record PetRegistered(PetId PetId, TenantId TenantId, TutorId TutorId) : IDomainEvent;
