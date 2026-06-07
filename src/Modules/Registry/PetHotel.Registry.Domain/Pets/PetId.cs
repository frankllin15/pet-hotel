namespace PetHotel.Registry.Domain.Pets;

/// <summary>Identificador tipado de pet.</summary>
public readonly record struct PetId(Guid Value)
{
    public static PetId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
