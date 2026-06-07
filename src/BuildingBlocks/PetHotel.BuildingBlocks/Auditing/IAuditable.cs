namespace PetHotel.BuildingBlocks.Auditing;

/// <summary>
/// Entidade auditável. Campos preenchidos pelo interceptor de SaveChanges (docs/04).
/// </summary>
public interface IAuditable
{
    DateTimeOffset CreatedAt { get; }
    string? CreatedBy { get; }
    DateTimeOffset? UpdatedAt { get; }
    string? UpdatedBy { get; }
}
