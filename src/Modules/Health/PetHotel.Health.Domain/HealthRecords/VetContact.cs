using PetHotel.SharedKernel;

namespace PetHotel.Health.Domain.HealthRecords;

/// <summary>
/// Contato do veterinário particular do pet. Persistido como JSON dentro do agregado
/// (owned), por isso é classe com setters privados (mesmo padrão de EmergencyContact).
/// </summary>
public sealed class VetContact
{
    public string Name { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    /// <summary>Clínica/hospital onde atende. Opcional.</summary>
    public string? Clinic { get; private set; }

    private VetContact() { } // EF

    private VetContact(string name, string phone, string? clinic)
    {
        Name = name;
        Phone = phone;
        Clinic = clinic;
    }

    public static Result<VetContact> Create(string? name, string? phone, string? clinic)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation("vet_contact.name_required", "Nome do veterinário é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(phone))
        {
            return Error.Validation("vet_contact.phone_required", "Telefone do veterinário é obrigatório.");
        }

        return new VetContact(
            name.Trim(),
            phone.Trim(),
            string.IsNullOrWhiteSpace(clinic) ? null : clinic.Trim());
    }
}
