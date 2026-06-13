using PetHotel.Operations.Domain.Medications;
using PetHotel.Operations.Domain.Ports;

namespace PetHotel.Operations.Infrastructure.Persistence.Repositories;

/// <summary>Repositório do agregado <see cref="MedicationAdministration"/>.</summary>
public sealed class MedicationRepository(OperationsDbContext dbContext) : IMedicationRepository
{
    public void Add(MedicationAdministration administration) => dbContext.Medications.Add(administration);
}
