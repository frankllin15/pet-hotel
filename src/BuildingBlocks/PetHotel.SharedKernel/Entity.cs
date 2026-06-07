namespace PetHotel.SharedKernel;

/// <summary>
/// Entidade com identidade tipada. Não conhece infraestrutura (docs/01).
/// </summary>
public abstract class Entity<TId>
    where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    protected Entity(TId id) => Id = id;

    // Construtor sem parâmetros para o EF materializar.
    protected Entity() { }
}
