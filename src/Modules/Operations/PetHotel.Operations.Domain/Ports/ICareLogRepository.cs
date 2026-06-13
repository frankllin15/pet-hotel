using PetHotel.Operations.Domain.CareLog;

namespace PetHotel.Operations.Domain.Ports;

/// <summary>Porta de saída para o agregado <see cref="CareLogEntry"/>.</summary>
public interface ICareLogRepository
{
    void Add(CareLogEntry entry);
}
