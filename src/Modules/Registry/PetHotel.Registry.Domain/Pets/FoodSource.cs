namespace PetHotel.Registry.Domain.Pets;

/// <summary>Origem da ração durante a hospedagem.</summary>
public enum FoodSource
{
    /// <summary>O tutor traz a ração do pet.</summary>
    TutorProvided = 1,

    /// <summary>O hotel fornece a ração.</summary>
    HotelProvided = 2,
}
