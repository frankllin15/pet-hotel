namespace PetHotel.Health.Domain.HealthRecords;

/// <summary>
/// Resultado da avaliação de aptidão sanitária do pet em uma data: apto quando não
/// há vacina obrigatória ausente ou vencida (docs/03).
/// </summary>
public sealed record HealthClearance(bool IsCleared, IReadOnlyList<VaccineType> Pendencies)
{
    public static HealthClearance Clear() => new(true, []);
}
