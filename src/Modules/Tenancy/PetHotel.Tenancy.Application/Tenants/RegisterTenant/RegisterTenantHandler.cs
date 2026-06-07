using FluentValidation;
using PetHotel.SharedKernel;
using PetHotel.Tenancy.Application.Abstractions;
using PetHotel.Tenancy.Application.Validation;
using PetHotel.Tenancy.Domain.Ports;
using PetHotel.Tenancy.Domain.Tenants;

namespace PetHotel.Tenancy.Application.Tenants.RegisterTenant;

/// <summary>
/// Handler fino: valida o input, respeita a invariante de unicidade de slug e
/// delega a criação ao agregado (docs/02).
/// </summary>
public static class RegisterTenantHandler
{
    public static async Task<Result<Guid>> Handle(
        RegisterTenant command,
        IValidator<RegisterTenant> validator,
        ITenantRepository tenants,
        IUnitOfWork unitOfWork,
        TimeProvider clock,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        var result = Tenant.Register(command.Name, command.Slug, clock.GetUtcNow());
        if (result.IsFailure)
        {
            return result.Error;
        }

        var tenant = result.Value;

        if (await tenants.SlugExistsAsync(tenant.Slug, cancellationToken))
        {
            return Error.Conflict("tenant.slug_taken", "Já existe um hotel com esse slug.");
        }

        tenants.Add(tenant);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return tenant.Id.Value;
    }
}
