using PetHotel.Operations.Application.Abstractions;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Application.Medications.GetDayMedications;

/// <summary>Projeta as medicações do dia direto para DTO (docs/04).</summary>
public static class GetDayMedicationsHandler
{
    public static async Task<Result<IReadOnlyList<DayMedicationDto>>> Handle(
        GetDayMedications query,
        IMedicationQueries medications,
        CancellationToken cancellationToken)
    {
        var rows = await medications.GetByDateAsync(query.Date, cancellationToken);
        return Result.Success(rows);
    }
}
