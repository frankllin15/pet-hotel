namespace PetHotel.Health.Domain.HealthRecords;

/// <summary>Identificador tipado de um controle de parasitas.</summary>
public readonly record struct ParasiteTreatmentId(Guid Value)
{
    public static ParasiteTreatmentId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
