using PetHotel.SharedKernel;

namespace PetHotel.Registry.Domain.Pets;

/// <summary>
/// Pertence que o pet traz para a hospedagem (coleira, brinquedo, cobertor, remédio,
/// etc.). Persistido como JSON dentro do agregado (owned), por isso é classe com
/// setters privados (mesmo padrão de AuthorizedPickup).
/// </summary>
public sealed class Belonging
{
    /// <summary>Descrição do item, ex.: "Coleira", "Cobertor azul".</summary>
    public string Name { get; private set; } = null!;
    /// <summary>Quantidade entregue (>= 1).</summary>
    public int Quantity { get; private set; }
    /// <summary>Observações (cor, marca, estado de conservação). Opcional.</summary>
    public string? Notes { get; private set; }

    private Belonging() { } // EF

    private Belonging(string name, int quantity, string? notes)
    {
        Name = name;
        Quantity = quantity;
        Notes = notes;
    }

    public static Result<Belonging> Create(string? name, int quantity, string? notes)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation("belonging.name_required", "Descrição do pertence é obrigatória.");
        }

        if (quantity < 1)
        {
            return Error.Validation("belonging.quantity_invalid", "Quantidade do pertence deve ser ao menos 1.");
        }

        return new Belonging(
            name.Trim(),
            quantity,
            string.IsNullOrWhiteSpace(notes) ? null : notes.Trim());
    }
}
