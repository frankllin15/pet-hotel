using System.Text.RegularExpressions;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Domain.Tutors;

/// <summary>
/// Telefone do tutor. Normaliza para dígitos com prefixo internacional opcional.
/// </summary>
public sealed partial record PhoneNumber : ValueObject
{
    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static Result<PhoneNumber> Create(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Error.Validation("phone.required", "Telefone é obrigatório.");
        }

        // Mantém apenas dígitos e um '+' inicial opcional.
        var trimmed = input.Trim();
        var hasPlus = trimmed.StartsWith('+');
        var digits = DigitsOnly().Replace(trimmed, string.Empty);

        if (digits.Length is < 8 or > 15)
        {
            return Error.Validation("phone.invalid", "Telefone deve ter entre 8 e 15 dígitos.");
        }

        return new PhoneNumber(hasPlus ? $"+{digits}" : digits);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"\D")]
    private static partial Regex DigitsOnly();
}
