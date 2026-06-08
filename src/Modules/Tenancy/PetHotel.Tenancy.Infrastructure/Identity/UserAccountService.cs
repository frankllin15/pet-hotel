using Microsoft.AspNetCore.Identity;
using PetHotel.SharedKernel;
using PetHotel.Tenancy.Application.Abstractions;

namespace PetHotel.Tenancy.Infrastructure.Identity;

/// <summary>
/// Implementa as operações de conta com ASP.NET Core Identity (UserManager/RoleManager).
/// Hash/lockout/tokens vêm do Identity (memória onboarding-and-auth).
/// </summary>
public sealed class UserAccountService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager) : IUserAccountService
{
    public async Task<Result<PendingAccount>> CreatePendingAsync(
        TenantId tenantId, string email, string displayName, string role, CancellationToken cancellationToken = default)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new ApplicationRole(role));
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            TenantId = tenantId,
            DisplayName = displayName,
            Status = UserAccountStatus.PendingActivation
        };

        var created = await userManager.CreateAsync(user);
        if (!created.Succeeded)
        {
            return ToError(created);
        }

        var assigned = await userManager.AddToRoleAsync(user, role);
        if (!assigned.Succeeded)
        {
            return ToError(assigned);
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        return new PendingAccount(user.Id, token);
    }

    public async Task<Result> ActivateAsync(
        string email, string token, string password, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Error.NotFound("account.not_found", "Conta não encontrada.");
        }

        var reset = await userManager.ResetPasswordAsync(user, token, password);
        if (!reset.Succeeded)
        {
            return Error.Validation("account.invalid_token", "Token inválido ou expirado, ou senha fraca.");
        }

        user.Status = UserAccountStatus.Active;
        var updated = await userManager.UpdateAsync(user);
        return updated.Succeeded ? Result.Success() : ToError(updated);
    }

    public async Task<Result<AuthenticatedUser>> AuthenticateAsync(
        string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Error.Unauthorized("auth.invalid_credentials", "E-mail ou senha inválidos.");
        }

        if (user.Status != UserAccountStatus.Active)
        {
            return Error.Forbidden("auth.not_active", "Conta ainda não ativada.");
        }

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            return Error.Unauthorized("auth.invalid_credentials", "E-mail ou senha inválidos.");
        }

        var roles = await userManager.GetRolesAsync(user);
        return new AuthenticatedUser(user.Id, user.TenantId.Value, user.Email!, user.DisplayName, roles.ToList());
    }

    private static Error ToError(IdentityResult result) =>
        Error.Validation("identity_error", string.Join("; ", result.Errors.Select(e => e.Description)));
}
