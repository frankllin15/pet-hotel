using FluentValidation;

namespace PetHotel.Tenancy.Application.Configuration;

/// <summary>Garante que o comando está bem formado (docs/02). Invariantes finas ficam no domínio.</summary>
public sealed class UpdateTenantConfigurationValidator : AbstractValidator<UpdateTenantConfiguration>
{
    public UpdateTenantConfigurationValidator()
    {
        RuleForEach(x => x.AccommodationTypes).ChildRules(type =>
        {
            type.RuleFor(t => t.Name).NotEmpty().MaximumLength(120);
            type.RuleFor(t => t.Capacity).GreaterThan(0);
            type.RuleFor(t => t.DailyPrice).GreaterThanOrEqualTo(0);
        });
    }
}
