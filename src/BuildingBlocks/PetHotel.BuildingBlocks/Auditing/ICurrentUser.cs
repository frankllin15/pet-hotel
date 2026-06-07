namespace PetHotel.BuildingBlocks.Auditing;

/// <summary>
/// Usuário corrente do request (para auditoria). Populado a partir do token.
/// </summary>
public interface ICurrentUser
{
    string? UserId { get; }
}
