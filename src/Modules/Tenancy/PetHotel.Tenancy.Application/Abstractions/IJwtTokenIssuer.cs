namespace PetHotel.Tenancy.Application.Abstractions;

/// <summary>Emite o JWT de acesso com os claims do usuário (tenant_id, role, sub).</summary>
public interface IJwtTokenIssuer
{
    AccessToken Issue(AuthenticatedUser user);
}

/// <summary>Token de acesso emitido.</summary>
public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);
