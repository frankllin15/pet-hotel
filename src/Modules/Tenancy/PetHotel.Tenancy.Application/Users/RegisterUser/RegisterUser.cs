using PetHotel.Tenancy.Domain.Users;

namespace PetHotel.Tenancy.Application.Users.RegisterUser;

/// <summary>Registra um usuário no tenant corrente (resolvido pelo token, docs/04).</summary>
public sealed record RegisterUser(string Email, string DisplayName, UserRole Role);
