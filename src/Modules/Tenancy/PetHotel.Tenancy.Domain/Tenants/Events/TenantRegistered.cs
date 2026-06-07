using PetHotel.SharedKernel;

namespace PetHotel.Tenancy.Domain.Tenants.Events;

/// <summary>Um novo tenant (hotel) foi registrado.</summary>
public sealed record TenantRegistered(TenantId TenantId, string Slug) : IDomainEvent;
