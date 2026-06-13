namespace PetHotel.Operations.Domain.Incidents;

/// <summary>Gravidade do incidente.</summary>
public enum IncidentSeverity
{
    /// <summary>Leve (sem risco; registro informativo).</summary>
    Low = 1,

    /// <summary>Moderado (requer atenção/acompanhamento).</summary>
    Medium = 2,

    /// <summary>Grave (risco; aciona o tutor).</summary>
    High = 3,
}
