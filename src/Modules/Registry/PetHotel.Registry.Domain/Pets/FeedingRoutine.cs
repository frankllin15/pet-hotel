using PetHotel.SharedKernel;

namespace PetHotel.Registry.Domain.Pets;

/// <summary>
/// Rotina alimentar do pet: ração, quantidade por refeição, horários, restrições
/// e origem da ração. Persistida como JSON dentro do agregado (owned), por isso é
/// classe com setters privados (mesmo padrão de EmergencyContact).
/// </summary>
public sealed class FeedingRoutine
{
    /// <summary>Ração (marca/tipo), ex.: "Golden Filhote".</summary>
    public string FoodName { get; private set; } = null!;
    /// <summary>Quantidade por refeição, em texto livre (ex.: "100 g", "1 xícara").</summary>
    public string? PortionSize { get; private set; }
    /// <summary>Horários das refeições, ordenados e sem repetição.</summary>
    public List<TimeOnly> MealTimes { get; private set; } = [];
    /// <summary>Restrições alimentares (alergias, alimentos proibidos).</summary>
    public string? Restrictions { get; private set; }
    public FoodSource FoodSource { get; private set; }

    private FeedingRoutine() { } // EF

    private FeedingRoutine(
        string foodName,
        string? portionSize,
        List<TimeOnly> mealTimes,
        string? restrictions,
        FoodSource foodSource)
    {
        FoodName = foodName;
        PortionSize = portionSize;
        MealTimes = mealTimes;
        Restrictions = restrictions;
        FoodSource = foodSource;
    }

    public static Result<FeedingRoutine> Create(
        string? foodName,
        string? portionSize,
        IEnumerable<TimeOnly>? mealTimes,
        string? restrictions,
        FoodSource foodSource)
    {
        if (string.IsNullOrWhiteSpace(foodName))
        {
            return Error.Validation("feeding_routine.food_name_required", "Ração é obrigatória na rotina alimentar.");
        }

        if (!Enum.IsDefined(foodSource))
        {
            return Error.Validation("feeding_routine.food_source_invalid", "Origem da ração inválida.");
        }

        return new FeedingRoutine(
            foodName.Trim(),
            string.IsNullOrWhiteSpace(portionSize) ? null : portionSize.Trim(),
            mealTimes?.Distinct().Order().ToList() ?? [],
            string.IsNullOrWhiteSpace(restrictions) ? null : restrictions.Trim(),
            foodSource);
    }
}
