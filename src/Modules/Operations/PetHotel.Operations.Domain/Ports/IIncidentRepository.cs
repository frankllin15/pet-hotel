using PetHotel.Operations.Domain.Incidents;

namespace PetHotel.Operations.Domain.Ports;

/// <summary>Porta de saída do agregado <see cref="Incident"/>.</summary>
public interface IIncidentRepository
{
    void Add(Incident incident);
}
