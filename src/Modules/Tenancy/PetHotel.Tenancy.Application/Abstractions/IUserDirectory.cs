using PetHotel.SharedKernel;

namespace PetHotel.Tenancy.Application.Abstractions;

/// <summary>Lista usuários de um tenant (para a administração da equipe).</summary>
public interface IUserDirectory
{
    Task<IReadOnlyList<DirectoryUser>> ListByTenantAsync(TenantId tenant, CancellationToken cancellationToken = default);
}

/// <summary>Usuário na visão de administração.</summary>
public sealed record DirectoryUser(Guid Id, string Email, string DisplayName, string Status, IReadOnlyList<string> Roles);
