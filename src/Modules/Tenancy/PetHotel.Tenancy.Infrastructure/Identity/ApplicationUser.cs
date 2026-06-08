using Microsoft.AspNetCore.Identity;
using PetHotel.SharedKernel;

namespace PetHotel.Tenancy.Infrastructure.Identity;

/// <summary>
/// Usuário de autenticação (store do ASP.NET Core Identity). Pertence a um tenant.
/// O Identity resolve hash de senha, lockout, MFA, tokens etc. (memória onboarding-and-auth).
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public TenantId TenantId { get; set; }
    public string DisplayName { get; set; } = null!;
    public UserAccountStatus Status { get; set; } = UserAccountStatus.PendingActivation;
}

/// <summary>Situação da conta. Materializa "convite pendente" vs "ativo".</summary>
public enum UserAccountStatus
{
    PendingActivation,
    Active,
    Disabled
}
