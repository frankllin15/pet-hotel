using FluentValidation;
using PetHotel.SharedKernel;
using PetHotel.Tenancy.Application.Abstractions;
using PetHotel.Tenancy.Application.Validation;
using PetHotel.Tenancy.Domain.Configuration;
using PetHotel.Tenancy.Domain.Ports;
using PetHotel.Tenancy.Domain.Tenants;

namespace PetHotel.Tenancy.Application.Provisioning;

/// <summary>
/// Operação de plataforma (sem tenant no contexto): cria atomicamente Tenant +
/// TenantConfiguration (defaults) + usuário admin pendente com papel Owner. O save do
/// admin (Identity, mesmo DbContext) persiste tenant e config na mesma transação.
/// </summary>
public static class ProvisionTenantHandler
{
    public static async Task<Result<ProvisionedTenant>> Handle(
        ProvisionTenant command,
        IValidator<ProvisionTenant> validator,
        ITenantRepository tenants,
        ITenantConfigurationRepository configurations,
        IUserAccountService userAccounts,
        TimeProvider clock,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        var tenantResult = Tenant.Register(command.Name, command.Slug, clock.GetUtcNow());
        if (tenantResult.IsFailure)
        {
            return tenantResult.Error;
        }

        var tenant = tenantResult.Value;

        if (await tenants.SlugExistsAsync(tenant.Slug, cancellationToken))
        {
            return Error.Conflict("tenant.slug_taken", "Já existe um hotel com esse slug.");
        }

        tenants.Add(tenant);
        configurations.Add(TenantConfiguration.CreateDefaults(tenant.Id));

        var pending = await userAccounts.CreatePendingAsync(
            tenant.Id, command.AdminEmail, command.AdminDisplayName, Roles.Owner, cancellationToken);
        if (pending.IsFailure)
        {
            return pending.Error;
        }

        return new ProvisionedTenant(tenant.Id.Value, pending.Value.UserId, pending.Value.ActivationToken);
    }
}
