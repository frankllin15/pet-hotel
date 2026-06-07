using Microsoft.EntityFrameworkCore;
using PetHotel.Tenancy.Domain.Ports;
using PetHotel.Tenancy.Domain.Users;

namespace PetHotel.Tenancy.Infrastructure.Persistence.Repositories;

/// <summary>Repositório do agregado <see cref="User"/>. Leituras já filtradas por tenant.</summary>
public sealed class UserRepository(TenancyDbContext dbContext) : IUserRepository
{
    public Task<User?> FindAsync(UserId id, CancellationToken cancellationToken = default) =>
        dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default) =>
        dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public void Add(User user) => dbContext.Users.Add(user);
}
