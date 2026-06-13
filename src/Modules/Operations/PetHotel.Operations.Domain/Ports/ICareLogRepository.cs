using PetHotel.Operations.Domain.CareLog;

namespace PetHotel.Operations.Domain.Ports;

/// <summary>Porta de saída para o agregado <see cref="CareLogEntry"/>.</summary>
public interface ICareLogRepository
{
    Task<CareLogEntry?> FindAsync(CareLogEntryId id, CancellationToken cancellationToken = default);

    void Add(CareLogEntry entry);
}
