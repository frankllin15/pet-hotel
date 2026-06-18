using PetHotel.Operations.Domain.Tasks;

namespace PetHotel.Operations.Domain.Ports;

/// <summary>Porta de saída do agregado <see cref="OperationalTask"/>.</summary>
public interface IOperationalTaskRepository
{
    void Add(OperationalTask task);

    Task<OperationalTask?> FindAsync(OperationalTaskId id, CancellationToken cancellationToken = default);

    void Remove(OperationalTask task);
}
