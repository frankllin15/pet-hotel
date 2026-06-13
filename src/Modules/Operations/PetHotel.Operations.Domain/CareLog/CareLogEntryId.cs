namespace PetHotel.Operations.Domain.CareLog;

/// <summary>Identificador tipado de uma entrada do diário de bordo.</summary>
public readonly record struct CareLogEntryId(Guid Value)
{
    public static CareLogEntryId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
