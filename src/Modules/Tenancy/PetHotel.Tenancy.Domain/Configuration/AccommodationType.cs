using PetHotel.SharedKernel;

namespace PetHotel.Tenancy.Domain.Configuration;

/// <summary>
/// Tipo de acomodação ofertado pelo hotel (parte da configuração). Persistido como
/// JSON dentro do agregado (owned), por isso é classe com setters privados.
/// </summary>
public sealed class AccommodationType
{
    public string Name { get; private set; } = null!;
    public int Capacity { get; private set; }
    public decimal DailyPrice { get; private set; }

    private AccommodationType() { } // EF

    private AccommodationType(string name, int capacity, decimal dailyPrice)
    {
        Name = name;
        Capacity = capacity;
        DailyPrice = dailyPrice;
    }

    public static Result<AccommodationType> Create(string? name, int capacity, decimal dailyPrice)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation("accommodation_type.name_required", "Nome do tipo de acomodação é obrigatório.");
        }

        if (capacity <= 0)
        {
            return Error.Validation("accommodation_type.capacity_invalid", "Capacidade deve ser maior que zero.");
        }

        if (dailyPrice < 0)
        {
            return Error.Validation("accommodation_type.price_invalid", "Preço diário não pode ser negativo.");
        }

        return new AccommodationType(name.Trim(), capacity, dailyPrice);
    }
}
