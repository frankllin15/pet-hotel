using Microsoft.AspNetCore.Identity;

namespace PetHotel.Tenancy.Infrastructure.Identity;

/// <summary>Papel (RBAC) do Identity. Nomes em <c>Tenancy.Application.Roles</c>.</summary>
public sealed class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }

    public ApplicationRole(string name) : base(name) { }
}
