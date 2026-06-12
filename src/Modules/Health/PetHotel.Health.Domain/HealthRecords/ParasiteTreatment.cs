using PetHotel.SharedKernel;

namespace PetHotel.Health.Domain.HealthRecords;

/// <summary>
/// Controle de parasitas (antipulgas/vermífugo) registrado na ficha. Entidade interna
/// do agregado <see cref="HealthRecord"/>: só é acessada pela raiz, que já é filtrada
/// por tenant — por isso não carrega TenantId próprio (mesmo padrão de Vaccination).
/// </summary>
public sealed class ParasiteTreatment : Entity<ParasiteTreatmentId>
{
    public ParasiteTreatmentType Type { get; private set; }
    /// <summary>Produto aplicado (ex.: "Bravecto", "Drontal"). Opcional.</summary>
    public string? ProductName { get; private set; }
    public DateOnly AppliedOn { get; private set; }
    /// <summary>Próxima dose prevista. Opcional (nem todo produto tem recorrência conhecida).</summary>
    public DateOnly? NextDueOn { get; private set; }

    private ParasiteTreatment() { } // EF

    internal ParasiteTreatment(
        ParasiteTreatmentId id,
        ParasiteTreatmentType type,
        string? productName,
        DateOnly appliedOn,
        DateOnly? nextDueOn) : base(id)
    {
        Type = type;
        ProductName = productName;
        AppliedOn = appliedOn;
        NextDueOn = nextDueOn;
    }

    /// <summary>Em dia na data informada? Null quando a próxima dose não foi informada.</summary>
    public bool? IsUpToDateOn(DateOnly date) => NextDueOn is { } due ? date <= due : null;

    /// <summary>Edita os dados do controle de parasitas.</summary>
    internal void Update(ParasiteTreatmentType type, string? productName, DateOnly appliedOn, DateOnly? nextDueOn)
    {
        Type = type;
        ProductName = string.IsNullOrWhiteSpace(productName) ? null : productName.Trim();
        AppliedOn = appliedOn;
        NextDueOn = nextDueOn;
    }
}
