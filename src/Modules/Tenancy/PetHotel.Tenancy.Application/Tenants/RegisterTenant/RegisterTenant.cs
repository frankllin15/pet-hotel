namespace PetHotel.Tenancy.Application.Tenants.RegisterTenant;

/// <summary>Registra um novo hotel (tenant). Operação de nível de plataforma.</summary>
public sealed record RegisterTenant(string Name, string Slug);
