using PetHotel.Health.Domain.HealthRecords;

namespace PetHotel.Health.Domain.Ports;

/// <summary>Porta de saída para persistência do agregado <see cref="HealthRecord"/>.</summary>
public interface IHealthRecordRepository
{
    /// <summary>Carrega a ficha do pet (com as vacinações) no tenant corrente.</summary>
    Task<HealthRecord?> FindByPetAsync(PetReference pet, CancellationToken cancellationToken = default);

    void Add(HealthRecord healthRecord);
}
