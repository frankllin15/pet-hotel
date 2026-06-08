using PetHotel.SharedKernel;

namespace PetHotel.Tenancy.Application.Abstractions;

/// <summary>
/// Porta para o store de identidade (impl. com ASP.NET Core Identity na Infrastructure).
/// Mantém a Application livre de detalhes de Identity (memória onboarding-and-auth).
/// </summary>
public interface IUserAccountService
{
    /// <summary>Cria um usuário pendente (sem senha) com o papel dado e devolve o token de ativação.</summary>
    Task<Result<PendingAccount>> CreatePendingAsync(
        TenantId tenantId, string email, string displayName, string role, CancellationToken cancellationToken = default);

    /// <summary>Ativa a conta definindo a senha a partir do token (single-use, com expiração).</summary>
    Task<Result> ActivateAsync(string email, string token, string password, CancellationToken cancellationToken = default);

    /// <summary>Valida credenciais e devolve os dados para o token de acesso.</summary>
    Task<Result<AuthenticatedUser>> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);
}

/// <summary>Conta recém-criada pendente de ativação.</summary>
public sealed record PendingAccount(Guid UserId, string ActivationToken);

/// <summary>Usuário autenticado, base para os claims do token.</summary>
public sealed record AuthenticatedUser(
    Guid Id, Guid TenantId, string Email, string DisplayName, IReadOnlyList<string> Roles);
