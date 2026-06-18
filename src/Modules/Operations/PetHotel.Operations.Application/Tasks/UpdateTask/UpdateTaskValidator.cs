using FluentValidation;

namespace PetHotel.Operations.Application.Tasks.UpdateTask;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class UpdateTaskValidator : AbstractValidator<UpdateTask>
{
    public UpdateTaskValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).IsInEnum();
    }
}
