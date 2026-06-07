using PetHotel.SharedKernel;

namespace PetHotel.Tenancy.Domain.Users.Events;

/// <summary>Um novo usuário foi registrado em um tenant.</summary>
public sealed record UserRegistered(UserId UserId, TenantId TenantId, string Email) : IDomainEvent;
