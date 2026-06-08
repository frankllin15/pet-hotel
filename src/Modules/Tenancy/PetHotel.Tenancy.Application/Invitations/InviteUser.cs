namespace PetHotel.Tenancy.Application.Invitations;

/// <summary>Convida um usuário para o tenant corrente, já com o papel (RBAC) atribuído.</summary>
public sealed record InviteUser(string Email, string DisplayName, string Role);

/// <summary>Resultado do convite (token vira o link de primeiro acesso).</summary>
public sealed record Invitation(Guid UserId, string ActivationToken);
