namespace PetHotel.SharedKernel;

/// <summary>
/// Raiz de agregado: fronteira de transação e de consistência (docs/03).
/// Acumula eventos de domínio que serão despachados após o commit.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _events = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _events.AsReadOnly();

    protected void Raise(IDomainEvent domainEvent) => _events.Add(domainEvent);

    public void ClearEvents() => _events.Clear();

    protected AggregateRoot(TId id) : base(id) { }

    protected AggregateRoot() { }
}
