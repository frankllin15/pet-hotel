namespace PetHotel.SharedKernel;

/// <summary>
/// Entidade auditável. Campos preenchidos pelo interceptor de SaveChanges (docs/04).
/// Contrato puro (sem EF), implementável pelo domínio.
/// </summary>
public interface IAuditable
{
    DateTimeOffset CreatedAt { get; }
    string? CreatedBy { get; }
    DateTimeOffset? UpdatedAt { get; }
    string? UpdatedBy { get; }
}
