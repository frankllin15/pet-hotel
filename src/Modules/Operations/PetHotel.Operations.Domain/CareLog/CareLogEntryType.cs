namespace PetHotel.Operations.Domain.CareLog;

/// <summary>Tipo da ocorrência registrada no diário de bordo do pet.</summary>
public enum CareLogEntryType
{
    /// <summary>Alimentação (comeu, recusou, petisco).</summary>
    Meal = 1,

    /// <summary>Necessidades fisiológicas (xixi/fezes).</summary>
    Bathroom = 2,

    /// <summary>Recreação / brincadeira.</summary>
    Play = 3,

    /// <summary>Comportamento observado.</summary>
    Behavior = 4,

    /// <summary>Higiene / limpeza.</summary>
    Hygiene = 5,

    /// <summary>Observação geral.</summary>
    Note = 6,
}
