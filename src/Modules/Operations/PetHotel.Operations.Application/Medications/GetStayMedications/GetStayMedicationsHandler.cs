using PetHotel.Operations.Application.Abstractions;
using PetHotel.Operations.Application.Medications;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Application.Medications.GetStayMedications;

/// <summary>Delega para a porta de leitura (docs/04).</summary>
public static class GetStayMedicationsHandler
{
    public static async Task<Result<IReadOnlyList<MedicationDto>>> Handle(
        GetStayMedications query,
        IMedicationQueries queries,
        CancellationToken cancellationToken)
    {
        return Result.Success(await queries.GetByContextAsync(query.ReservationId, cancellationToken));
    }
}
