namespace PetHotel.Health.Domain.HealthRecords;

/// <summary>Identificador tipado da ficha de saúde.</summary>
public readonly record struct HealthRecordId(Guid Value)
{
    public static HealthRecordId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
