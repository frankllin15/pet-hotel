using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PetHotel.Tenancy.Application.Abstractions;

namespace PetHotel.Tenancy.Infrastructure.Auth;

/// <summary>
/// Emite o JWT (HS256) com os claims lidos pelo contexto de request: <c>tenant_id</c>
/// (HttpTenantContext), <c>sub</c> (HttpCurrentUser) e <c>role</c> (RBAC).
/// </summary>
public sealed class JwtTokenIssuer(IOptions<JwtOptions> options, TimeProvider clock) : IJwtTokenIssuer
{
    private readonly JwtOptions _options = options.Value;

    public AccessToken Issue(AuthenticatedUser user)
    {
        var now = clock.GetUtcNow();
        var expiresAt = now.AddMinutes(_options.LifetimeMinutes);

        var claims = new List<Claim>
        {
            new("sub", user.Id.ToString()),
            new("tenant_id", user.TenantId.ToString()),
            new("email", user.Email),
            new("name", user.DisplayName)
        };
        claims.AddRange(user.Roles.Select(role => new Claim("role", role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return new AccessToken(jwt, expiresAt);
    }
}
