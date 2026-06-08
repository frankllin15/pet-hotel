namespace PetHotel.Tenancy.Application.Abstractions;

/// <summary>Papéis (RBAC) do sistema. Atribuídos no convite (memória onboarding-and-auth).</summary>
public static class Roles
{
    public const string Owner = "Owner";
    public const string Manager = "Manager";
    public const string Staff = "Staff";

    public static readonly IReadOnlyList<string> All = [Owner, Manager, Staff];

    public static bool IsValid(string role) => All.Contains(role);
}
