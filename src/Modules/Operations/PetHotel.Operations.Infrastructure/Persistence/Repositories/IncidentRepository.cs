using PetHotel.Operations.Domain.Incidents;
using PetHotel.Operations.Domain.Ports;

namespace PetHotel.Operations.Infrastructure.Persistence.Repositories;

/// <summary>Repositório do agregado <see cref="Incident"/>.</summary>
public sealed class IncidentRepository(OperationsDbContext dbContext) : IIncidentRepository
{
    public void Add(Incident incident) => dbContext.Incidents.Add(incident);
}
