using PetHotel.SharedKernel;
using PetHotel.Tenancy.Domain.Tenants.Events;

namespace PetHotel.Tenancy.Domain.Tenants;

/// <summary>
/// Hotel de pets. Raiz de agregado de nível de plataforma — não é tenant-scoped
/// (é o próprio tenant). Invariantes de nome/slug vivem aqui (docs/03).
/// </summary>
public sealed class Tenant : AggregateRoot<TenantId>
{
    public string Name { get; private set; } = null!;
    public Slug Slug { get; private set; } = null!;
    public TenantStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Tenant() { } // EF

    private Tenant(TenantId id, string name, Slug slug, DateTimeOffset createdAt) : base(id)
    {
        Name = name;
        Slug = slug;
        Status = TenantStatus.Active;
        CreatedAt = createdAt;
    }

    public static Result<Tenant> Register(string? name, string? slug, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation("tenant.name_required", "Nome do tenant é obrigatório.");
        }

        var slugResult = Slug.Create(slug);
        if (slugResult.IsFailure)
        {
            return slugResult.Error;
        }

        var tenant = new Tenant(new TenantId(Guid.NewGuid()), name.Trim(), slugResult.Value, now);
        tenant.Raise(new TenantRegistered(tenant.Id, tenant.Slug.Value));
        return tenant;
    }

    public Result Suspend()
    {
        if (Status == TenantStatus.Suspended)
        {
            return Error.Conflict("tenant.already_suspended", "Tenant já está suspenso.");
        }

        Status = TenantStatus.Suspended;
        return Result.Success();
    }

    public Result Activate()
    {
        if (Status == TenantStatus.Active)
        {
            return Error.Conflict("tenant.already_active", "Tenant já está ativo.");
        }

        Status = TenantStatus.Active;
        return Result.Success();
    }

    public Result Rename(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation("tenant.name_required", "Nome do tenant é obrigatório.");
        }

        Name = name.Trim();
        return Result.Success();
    }
}
