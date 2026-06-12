namespace PetHotel.Booking.Domain.Reservations;

/// <summary>Condição geral do pet observada na chegada (check-in).</summary>
public enum ArrivalCondition
{
    /// <summary>Saudável, sem alterações aparentes.</summary>
    Healthy = 1,

    /// <summary>Alterações leves (sujeira, agitação, ferimento superficial).</summary>
    MinorIssues = 2,

    /// <summary>Requer atenção (abatido, machucado, sintomas).</summary>
    NeedsAttention = 3,
}
