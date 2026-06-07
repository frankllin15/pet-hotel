using PetHotel.SharedKernel;
using PetHotel.Registry.Domain.Tutors.Events;

namespace PetHotel.Registry.Domain.Tutors;

/// <summary>
/// Tutor (dono dos pets). Agregado tenant-scoped e auditável (docs/03).
/// </summary>
public sealed class Tutor : AggregateRoot<TutorId>, IHasTenant, IAuditable
{
    public TenantId TenantId { get; private set; }
    public string FullName { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public PhoneNumber Phone { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private Tutor() { } // EF

    private Tutor(TutorId id, TenantId tenantId, string fullName, Email email, PhoneNumber phone) : base(id)
    {
        TenantId = tenantId;
        FullName = fullName;
        Email = email;
        Phone = phone;
    }

    public static Result<Tutor> Register(TenantId tenantId, string? fullName, string? email, string? phone)
    {
        if (tenantId.Value == Guid.Empty)
        {
            return Error.Validation("tutor.tenant_required", "Tenant é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Error.Validation("tutor.name_required", "Nome do tutor é obrigatório.");
        }

        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
        {
            return emailResult.Error;
        }

        var phoneResult = PhoneNumber.Create(phone);
        if (phoneResult.IsFailure)
        {
            return phoneResult.Error;
        }

        var tutor = new Tutor(TutorId.New(), tenantId, fullName.Trim(), emailResult.Value, phoneResult.Value);
        tutor.Raise(new TutorRegistered(tutor.Id, tenantId, tutor.Email.Value));
        return tutor;
    }

    public Result UpdateContact(string? email, string? phone)
    {
        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
        {
            return emailResult.Error;
        }

        var phoneResult = PhoneNumber.Create(phone);
        if (phoneResult.IsFailure)
        {
            return phoneResult.Error;
        }

        Email = emailResult.Value;
        Phone = phoneResult.Value;
        return Result.Success();
    }
}
