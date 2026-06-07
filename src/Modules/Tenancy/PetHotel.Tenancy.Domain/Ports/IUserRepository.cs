using PetHotel.Tenancy.Domain.Users;

namespace PetHotel.Tenancy.Domain.Ports;

/// <summary>Porta de saída para persistência do agregado <see cref="User"/>.</summary>
public interface IUserRepository
{
    Task<User?> FindAsync(UserId id, CancellationToken cancellationToken = default);

    /// <summary>Verifica duplicidade de e-mail dentro do tenant corrente (query filter aplicado).</summary>
    Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default);

    void Add(User user);
}
