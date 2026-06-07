using PetHotel.Tenancy.Application.Abstractions;

namespace PetHotel.Tenancy.Infrastructure.Persistence;

/// <summary>
/// Adaptador concreto de <see cref="IUnitOfWork"/> sobre o <see cref="TenancyDbContext"/>.
/// Tipo concreto (sem factory lambda) para o codegen do Wolverine resolver sem service location.
/// </summary>
public sealed class TenancyUnitOfWork(TenancyDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
