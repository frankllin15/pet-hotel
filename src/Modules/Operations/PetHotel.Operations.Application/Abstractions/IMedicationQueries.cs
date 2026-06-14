using PetHotel.Operations.Application.Medications;

namespace PetHotel.Operations.Application.Abstractions;

/// <summary>Porta de leitura das medicações (AsNoTracking + query filter de tenant, docs/04).</summary>
public interface IMedicationQueries
{
    /// <summary>Medicações de uma estadia, mais recentes primeiro.</summary>
    Task<IReadOnlyList<MedicationDto>> GetByContextAsync(Guid contextId, CancellationToken cancellationToken = default);

    /// <summary>Medicações administradas no dia informado (todas as estadias do tenant), por horário.</summary>
    Task<IReadOnlyList<DayMedicationDto>> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
}
