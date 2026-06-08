using PetHotel.SharedKernel;

namespace PetHotel.Tenancy.Domain.Configuration;

/// <summary>
/// Configuração operacional de um hotel (uma por tenant). Preenchida no wizard de
/// setup; <see cref="SetupCompleted"/> indica se o mínimo para operar existe (docs/03).
/// </summary>
public sealed class TenantConfiguration : AggregateRoot<TenantConfigurationId>, IHasTenant, IAuditable
{
    public TenantId TenantId { get; private set; }
    public List<AccommodationType> AccommodationTypes { get; private set; } = [];
    public List<string> RequiredVaccines { get; private set; } = [];
    public TimeOnly CheckInTime { get; private set; }
    public TimeOnly CheckOutTime { get; private set; }
    public bool SetupCompleted { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private TenantConfiguration() { } // EF

    private TenantConfiguration(TenantConfigurationId id, TenantId tenantId) : base(id) => TenantId = tenantId;

    /// <summary>Cria a configuração com defaults sensatos (usado no provisionamento).</summary>
    public static TenantConfiguration CreateDefaults(TenantId tenantId) =>
        new(TenantConfigurationId.New(), tenantId)
        {
            RequiredVaccines = ["Rabies"],
            CheckInTime = new TimeOnly(14, 0),
            CheckOutTime = new TimeOnly(12, 0),
            SetupCompleted = false
        };

    /// <summary>
    /// Aplica o wizard de setup. Considera o setup concluído quando há pelo menos um
    /// tipo de acomodação (mínimo para operar).
    /// </summary>
    public Result Update(
        IEnumerable<AccommodationType> accommodationTypes,
        IEnumerable<string> requiredVaccines,
        TimeOnly checkInTime,
        TimeOnly checkOutTime)
    {
        AccommodationTypes = accommodationTypes.ToList();
        RequiredVaccines = requiredVaccines
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.Trim())
            .Distinct()
            .ToList();
        CheckInTime = checkInTime;
        CheckOutTime = checkOutTime;
        SetupCompleted = AccommodationTypes.Count > 0;
        return Result.Success();
    }
}
