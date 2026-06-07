using FluentValidation;

namespace PetHotel.Tenancy.Application.Tenants.RegisterTenant;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class RegisterTenantValidator : AbstractValidator<RegisterTenant>
{
    public RegisterTenantValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(63);
    }
}
