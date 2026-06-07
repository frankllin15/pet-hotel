namespace PetHotel.Tenancy.Domain.Users;

/// <summary>Identificador tipado de usuário.</summary>
public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
