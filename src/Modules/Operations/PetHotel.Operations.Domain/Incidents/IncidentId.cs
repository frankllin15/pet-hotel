namespace PetHotel.Operations.Domain.Incidents;

/// <summary>Identificador tipado de um incidente.</summary>
public readonly record struct IncidentId(Guid Value)
{
    public static IncidentId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
