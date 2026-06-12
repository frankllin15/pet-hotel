using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PetHotel.LoadTest.Seeder;

/// <summary>
/// Emite um JWT HS256 com os mesmos claims do issuer real (sub, tenant_id, role) para
/// que o k6 chame os endpoints tenant-scoped sem precisar do fluxo de login/Identity.
/// O token é self-contained: a API valida assinatura/issuer/audience e lê o tenant do
/// claim, sem consultar o banco — então não é preciso criar usuários Identity.
/// </summary>
public sealed class TokenMinter(string issuer, string audience, string signingKey)
{
    private readonly SigningCredentials _credentials = new(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)), SecurityAlgorithms.HmacSha256);

    public string Issue(Guid tenantId, TimeSpan lifetime)
    {
        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims:
            [
                new Claim("sub", Guid.NewGuid().ToString()),
                new Claim("tenant_id", tenantId.ToString()),
                new Claim("email", $"loadtest-{tenantId:N}@example.com"),
                new Claim("name", "Load Test"),
                new Claim("role", "Owner"),
            ],
            notBefore: now,
            expires: now.Add(lifetime),
            signingCredentials: _credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
