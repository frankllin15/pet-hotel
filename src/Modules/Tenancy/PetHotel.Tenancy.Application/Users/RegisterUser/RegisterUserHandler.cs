using FluentValidation;
using PetHotel.SharedKernel;
using PetHotel.Tenancy.Application.Abstractions;
using PetHotel.Tenancy.Application.Validation;
using PetHotel.Tenancy.Domain.Ports;
using PetHotel.Tenancy.Domain.Users;

namespace PetHotel.Tenancy.Application.Users.RegisterUser;

/// <summary>
/// Cria um usuário no tenant corrente. O tenant vem do token (ITenantContext),
/// nunca do payload (docs/04).
/// </summary>
public static class RegisterUserHandler
{
    public static async Task<Result<Guid>> Handle(
        RegisterUser command,
        IValidator<RegisterUser> validator,
        ITenantContext tenantContext,
        IUserRepository users,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        if (!tenantContext.HasTenant)
        {
            return Error.Forbidden("tenant.required", "A operação exige um tenant no contexto.");
        }

        var result = User.Register(tenantContext.Current, command.Email, command.DisplayName, command.Role);
        if (result.IsFailure)
        {
            return result.Error;
        }

        var user = result.Value;

        if (await users.EmailExistsAsync(user.Email, cancellationToken))
        {
            return Error.Conflict("user.email_taken", "Já existe um usuário com esse e-mail neste hotel.");
        }

        users.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id.Value;
    }
}
