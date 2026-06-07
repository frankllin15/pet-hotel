using FluentValidation.Results;
using PetHotel.SharedKernel;

namespace PetHotel.Tenancy.Application.Validation;

/// <summary>
/// Converte falha de FluentValidation em <see cref="Error"/> de validação,
/// mantendo o fluxo baseado em Result (sem exceção, docs/02).
/// </summary>
public static class ValidationExtensions
{
    public static Error ToError(this ValidationResult result) =>
        Error.Validation("validation_failed", string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
}
