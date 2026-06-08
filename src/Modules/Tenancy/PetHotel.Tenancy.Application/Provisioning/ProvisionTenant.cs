namespace PetHotel.Tenancy.Application.Provisioning;

/// <summary>Provisiona um novo hotel: tenant + configuração default + usuário admin pendente.</summary>
public sealed record ProvisionTenant(string Name, string Slug, string AdminEmail, string AdminDisplayName);

/// <summary>Resultado do provisionamento (o token de ativação vira o link do primeiro acesso).</summary>
public sealed record ProvisionedTenant(Guid TenantId, Guid AdminUserId, string ActivationToken);
