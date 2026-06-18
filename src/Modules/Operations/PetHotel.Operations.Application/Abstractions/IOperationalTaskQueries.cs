using PetHotel.Operations.Application.Tasks;

namespace PetHotel.Operations.Application.Abstractions;

/// <summary>Porta de leitura das tarefas operacionais (AsNoTracking + query filter de tenant, docs/04).</summary>
public interface IOperationalTaskQueries
{
    /// <summary>Tarefas de um dia, agrupáveis por categoria (pendentes primeiro).</summary>
    Task<IReadOnlyList<OperationalTaskDto>> ListByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
}
