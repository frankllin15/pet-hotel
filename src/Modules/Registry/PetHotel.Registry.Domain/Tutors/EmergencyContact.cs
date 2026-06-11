using PetHotel.SharedKernel;

namespace PetHotel.Registry.Domain.Tutors;

/// <summary>
/// Contato de emergência do tutor. Persistido como JSON dentro do agregado (owned),
/// por isso é classe com setters privados (mesmo padrão de AccommodationType).
/// </summary>
public sealed class EmergencyContact
{
    public string Name { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    /// <summary>Vínculo com o tutor (ex.: cônjuge, vizinho). Opcional.</summary>
    public string? Relationship { get; private set; }

    private EmergencyContact() { } // EF

    private EmergencyContact(string name, string phone, string? relationship)
    {
        Name = name;
        Phone = phone;
        Relationship = relationship;
    }

    public static Result<EmergencyContact> Create(string? name, string? phone, string? relationship)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation("emergency_contact.name_required", "Nome do contato de emergência é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(phone))
        {
            return Error.Validation("emergency_contact.phone_required", "Telefone do contato de emergência é obrigatório.");
        }

        return new EmergencyContact(
            name.Trim(),
            phone.Trim(),
            string.IsNullOrWhiteSpace(relationship) ? null : relationship.Trim());
    }
}
