using PetHotel.Operations.Application.Incidents;

namespace PetHotel.Operations.Application.Abstractions;

/// <summary>Porta de leitura dos incidentes (AsNoTracking + query filter de tenant, docs/04).</summary>
public interface IIncidentQueries
{
    /// <summary>Incidentes de uma estadia, mais recentes primeiro.</summary>
    Task<IReadOnlyList<IncidentDto>> GetByContextAsync(Guid contextId, CancellationToken cancellationToken = default);
}
