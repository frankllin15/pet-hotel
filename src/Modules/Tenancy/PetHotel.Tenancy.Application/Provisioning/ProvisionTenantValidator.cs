using FluentValidation;

namespace PetHotel.Tenancy.Application.Provisioning;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class ProvisionTenantValidator : AbstractValidator<ProvisionTenant>
{
    public ProvisionTenantValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(63);
        RuleFor(x => x.AdminEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.AdminDisplayName).NotEmpty().MaximumLength(200);
    }
}
