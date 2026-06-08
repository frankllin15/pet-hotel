using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PetHotel.SharedKernel;
using PetHotel.Tenancy.Application.Abstractions;

namespace PetHotel.Tenancy.Infrastructure.Identity;

/// <summary>Lista usuários de um tenant (com seus papéis) para a administração da equipe.</summary>
public sealed class UserDirectory(UserManager<ApplicationUser> userManager) : IUserDirectory
{
    public async Task<IReadOnlyList<DirectoryUser>> ListByTenantAsync(
        TenantId tenant, CancellationToken cancellationToken = default)
    {
        var users = await userManager.Users
            .Where(u => u.TenantId == tenant)
            .ToListAsync(cancellationToken);

        var result = new List<DirectoryUser>(users.Count);
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(new DirectoryUser(user.Id, user.Email!, user.DisplayName, user.Status.ToString(), roles.ToList()));
        }

        return result;
    }
}
