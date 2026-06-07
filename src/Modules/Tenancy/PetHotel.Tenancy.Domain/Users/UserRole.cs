namespace PetHotel.Tenancy.Domain.Users;

/// <summary>Papel do usuário dentro do hotel (RBAC, docs/03).</summary>
public enum UserRole
{
    Owner,
    Manager,
    Staff
}
