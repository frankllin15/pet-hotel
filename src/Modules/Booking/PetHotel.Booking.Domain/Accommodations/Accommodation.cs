using PetHotel.SharedKernel;

namespace PetHotel.Booking.Domain.Accommodations;

/// <summary>
/// Unidade reservável (box, suíte). Agregado tenant-scoped. Concorrência otimista via
/// xmin para impedir overbooking sob confirmações concorrentes (docs/04).
/// </summary>
public sealed class Accommodation : AggregateRoot<AccommodationId>, IHasTenant, IAuditable
{
    public TenantId TenantId { get; private set; }
    public string Name { get; private set; } = null!;
    /// <summary>Valor da diária cobrada por esta acomodação (na moeda do hotel).</summary>
    public decimal DailyRate { get; private set; }
    public AccommodationStatus Status { get; private set; }

    /// <summary>Última confirmação. Alterado a cada reserva confirmada para forçar o
    /// check de concorrência (xmin) e serializar confirmações na mesma acomodação.</summary>
    public DateTimeOffset? LastBookedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private Accommodation() { } // EF

    private Accommodation(AccommodationId id, TenantId tenantId, string name, decimal dailyRate) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        DailyRate = dailyRate;
        Status = AccommodationStatus.Available;
    }

    public static Result<Accommodation> Create(TenantId tenantId, string? name, decimal dailyRate)
    {
        if (tenantId.Value == Guid.Empty)
        {
            return Error.Validation("accommodation.tenant_required", "Tenant é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation("accommodation.name_required", "Nome da acomodação é obrigatório.");
        }

        if (dailyRate < 0)
        {
            return Error.Validation("accommodation.daily_rate_invalid", "Valor da diária não pode ser negativo.");
        }

        return new Accommodation(AccommodationId.New(), tenantId, name.Trim(), dailyRate);
    }

    public bool IsAvailable => Status == AccommodationStatus.Available;

    /// <summary>Edita os dados editáveis (nome e diária); mesmas invariantes do cadastro.</summary>
    public Result Update(string? name, decimal dailyRate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation("accommodation.name_required", "Nome da acomodação é obrigatório.");
        }

        if (dailyRate < 0)
        {
            return Error.Validation("accommodation.daily_rate_invalid", "Valor da diária não pode ser negativo.");
        }

        Name = name.Trim();
        DailyRate = dailyRate;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (Status == AccommodationStatus.Inactive)
        {
            return Error.Conflict("accommodation.already_inactive", "Acomodação já está inativa.");
        }

        Status = AccommodationStatus.Inactive;
        return Result.Success();
    }

    public Result Activate()
    {
        if (Status == AccommodationStatus.Available)
        {
            return Error.Conflict("accommodation.already_active", "Acomodação já está ativa.");
        }

        Status = AccommodationStatus.Available;
        return Result.Success();
    }

    /// <summary>Registra a confirmação de uma reserva (toca a linha para o check de xmin).</summary>
    public void MarkBooked(DateTimeOffset at) => LastBookedAt = at;
}
