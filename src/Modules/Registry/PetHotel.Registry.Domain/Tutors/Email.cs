using System.Text.RegularExpressions;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Domain.Tutors;

/// <summary>
/// E-mail do tutor. VO próprio do contexto Registry (módulos não compartilham domínio, docs/01).
/// </summary>
public sealed partial record Email : ValueObject
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Result<Email> Create(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Error.Validation("email.required", "E-mail é obrigatório.");
        }

        var normalized = input.Trim().ToLowerInvariant();

        if (!EmailPattern().IsMatch(normalized))
        {
            return Error.Validation("email.invalid", "E-mail inválido.");
        }

        return new Email(normalized);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailPattern();
}
