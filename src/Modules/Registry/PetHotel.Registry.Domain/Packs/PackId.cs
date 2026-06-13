namespace PetHotel.Registry.Domain.Packs;

/// <summary>Identificador tipado de matilha.</summary>
public readonly record struct PackId(Guid Value)
{
    public static PackId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
