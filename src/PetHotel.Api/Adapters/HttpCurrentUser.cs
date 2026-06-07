using System.Security.Claims;
using PetHotel.BuildingBlocks.Auditing;

namespace PetHotel.Api.Adapters;

/// <summary>
/// Usuário corrente a partir do token, para auditoria (docs/04).
/// </summary>
public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public string? UserId =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? httpContextAccessor.HttpContext?.User.FindFirstValue("sub");
}
