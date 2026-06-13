namespace PetHotel.Registry.Domain.Packs;

/// <summary>
/// Pet pertencente a uma matilha — referência por Id (fronteira de agregado). Persistido
/// como JSON dentro do agregado (owned), por isso é classe com setter privado.
/// </summary>
public sealed class PackMember
{
    public Guid PetId { get; private set; }

    private PackMember() { } // EF

    public PackMember(Guid petId) => PetId = petId;
}
