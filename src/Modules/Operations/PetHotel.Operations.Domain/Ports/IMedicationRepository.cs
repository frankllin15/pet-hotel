using PetHotel.Operations.Domain.Medications;

namespace PetHotel.Operations.Domain.Ports;

/// <summary>Porta de saída do agregado <see cref="MedicationAdministration"/>.</summary>
public interface IMedicationRepository
{
    void Add(MedicationAdministration administration);
}
