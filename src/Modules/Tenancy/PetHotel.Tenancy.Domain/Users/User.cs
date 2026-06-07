using PetHotel.SharedKernel;
using PetHotel.Tenancy.Domain.Users.Events;

namespace PetHotel.Tenancy.Domain.Users;

/// <summary>
/// Usuário de um hotel. Pertence a um tenant (referência por Id) e é auditável.
/// </summary>
public sealed class User : AggregateRoot<UserId>, IHasTenant, IAuditable
{
    public TenantId TenantId { get; private set; }
    public Email Email { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public UserStatus Status { get; private set; }

    // Auditoria preenchida pelo interceptor (docs/04).
    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private User() { } // EF

    private User(UserId id, TenantId tenantId, Email email, string displayName, UserRole role) : base(id)
    {
        TenantId = tenantId;
        Email = email;
        DisplayName = displayName;
        Role = role;
        Status = UserStatus.Active;
    }

    public static Result<User> Register(TenantId tenantId, string? email, string? displayName, UserRole role)
    {
        if (tenantId.Value == Guid.Empty)
        {
            return Error.Validation("user.tenant_required", "Tenant é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Error.Validation("user.name_required", "Nome do usuário é obrigatório.");
        }

        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
        {
            return emailResult.Error;
        }

        var user = new User(UserId.New(), tenantId, emailResult.Value, displayName.Trim(), role);
        user.Raise(new UserRegistered(user.Id, tenantId, user.Email.Value));
        return user;
    }

    public Result ChangeRole(UserRole role)
    {
        if (Role == role)
        {
            return Error.Conflict("user.role_unchanged", "O usuário já possui esse papel.");
        }

        Role = role;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (Status == UserStatus.Inactive)
        {
            return Error.Conflict("user.already_inactive", "Usuário já está inativo.");
        }

        Status = UserStatus.Inactive;
        return Result.Success();
    }

    public Result Activate()
    {
        if (Status == UserStatus.Active)
        {
            return Error.Conflict("user.already_active", "Usuário já está ativo.");
        }

        Status = UserStatus.Active;
        return Result.Success();
    }
}
