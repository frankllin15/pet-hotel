using System.Text.RegularExpressions;
using PetHotel.SharedKernel;

namespace PetHotel.Tenancy.Domain.Tenants;

/// <summary>
/// Identificador legível e único do tenant (ex.: subdomínio). Imutável e auto-validado.
/// </summary>
public sealed partial record Slug : ValueObject
{
    public string Value { get; }

    private Slug(string value) => Value = value;

    public static Result<Slug> Create(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Error.Validation("slug.required", "Slug é obrigatório.");
        }

        var normalized = input.Trim().ToLowerInvariant();

        if (!SlugPattern().IsMatch(normalized))
        {
            return Error.Validation(
                "slug.invalid",
                "Slug deve conter apenas letras minúsculas, números e hífens, sem hífen nas pontas.");
        }

        return new Slug(normalized);
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[a-z0-9]([a-z0-9-]*[a-z0-9])?$")]
    private static partial Regex SlugPattern();
}
