using FluentValidation;

namespace PetHotel.Operations.Application.Tasks.CreateTask;

/// <summary>Garante que o comando está bem formado (docs/02).</summary>
public sealed class CreateTaskValidator : AbstractValidator<CreateTask>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).IsInEnum();
    }
}
