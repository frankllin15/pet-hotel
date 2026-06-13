using FluentValidation.Results;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Application.Validation;

/// <summary>Converte falha de FluentValidation em <see cref="Error"/> de validação (docs/02).</summary>
public static class ValidationExtensions
{
    public static Error ToError(this ValidationResult result) =>
        Error.Validation("validation_failed", string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
}
