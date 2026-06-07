namespace PetHotel.Tenancy.Application.Tenants;

/// <summary>Projeção de leitura de um tenant (lado de query, docs/04).</summary>
public sealed record TenantDto(Guid Id, string Name, string Slug, string Status, DateTimeOffset CreatedAt);
