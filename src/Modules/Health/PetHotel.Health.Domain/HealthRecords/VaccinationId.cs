namespace PetHotel.Health.Domain.HealthRecords;

/// <summary>Identificador tipado de uma vacinação.</summary>
public readonly record struct VaccinationId(Guid Value)
{
    public static VaccinationId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
