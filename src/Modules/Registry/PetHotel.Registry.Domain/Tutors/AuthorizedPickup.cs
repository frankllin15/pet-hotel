using PetHotel.SharedKernel;

namespace PetHotel.Registry.Domain.Tutors;

/// <summary>
/// Pessoa autorizada a retirar o pet em nome do tutor. Persistida como JSON dentro
/// do agregado (owned), por isso é classe com setters privados.
/// </summary>
public sealed class AuthorizedPickup
{
    public string Name { get; private set; } = null!;
    /// <summary>Documento de identificação (CPF/RG) para conferência na retirada. Opcional.</summary>
    public string? Document { get; private set; }

    private AuthorizedPickup() { } // EF

    private AuthorizedPickup(string name, string? document)
    {
        Name = name;
        Document = document;
    }

    public static Result<AuthorizedPickup> Create(string? name, string? document)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation("authorized_pickup.name_required", "Nome do autorizado a retirar é obrigatório.");
        }

        return new AuthorizedPickup(
            name.Trim(),
            string.IsNullOrWhiteSpace(document) ? null : document.Trim());
    }
}
