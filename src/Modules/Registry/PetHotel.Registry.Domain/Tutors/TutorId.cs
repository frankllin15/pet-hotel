namespace PetHotel.Registry.Domain.Tutors;

/// <summary>Identificador tipado de tutor.</summary>
public readonly record struct TutorId(Guid Value)
{
    public static TutorId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
