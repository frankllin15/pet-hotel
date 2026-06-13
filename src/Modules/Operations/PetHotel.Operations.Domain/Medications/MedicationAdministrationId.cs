namespace PetHotel.Operations.Domain.Medications;

/// <summary>Identificador tipado de uma administração de medicamento.</summary>
public readonly record struct MedicationAdministrationId(Guid Value)
{
    public static MedicationAdministrationId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
