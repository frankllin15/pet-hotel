using PetHotel.SharedKernel;

namespace PetHotel.Booking.Domain.Reservations;

/// <summary>
/// Estado do pet registrado na chegada (check-in): peso, condição geral e observações.
/// Documenta a condição de entrada (cuidado/responsabilidade). Persistido como JSON
/// dentro do agregado (owned), por isso é classe com setters privados.
/// </summary>
public sealed class ArrivalState
{
    /// <summary>Peso na chegada, em quilos. Opcional.</summary>
    public decimal? WeightKg { get; private set; }
    public ArrivalCondition Condition { get; private set; }
    /// <summary>Observações da recepção (marcas, ferimentos, comportamento na chegada). Opcional.</summary>
    public string? Observations { get; private set; }

    private ArrivalState() { } // EF

    private ArrivalState(decimal? weightKg, ArrivalCondition condition, string? observations)
    {
        WeightKg = weightKg;
        Condition = condition;
        Observations = observations;
    }

    public static Result<ArrivalState> Create(decimal? weightKg, ArrivalCondition condition, string? observations)
    {
        if (!Enum.IsDefined(condition))
        {
            return Error.Validation("arrival_state.condition_invalid", "Condição de chegada inválida.");
        }

        if (weightKg is <= 0)
        {
            return Error.Validation("arrival_state.weight_invalid", "Peso na chegada deve ser positivo.");
        }

        return new ArrivalState(
            weightKg,
            condition,
            string.IsNullOrWhiteSpace(observations) ? null : observations.Trim());
    }
}
