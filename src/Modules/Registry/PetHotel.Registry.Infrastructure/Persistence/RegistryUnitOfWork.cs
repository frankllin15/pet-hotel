using PetHotel.Registry.Application.Abstractions;

namespace PetHotel.Registry.Infrastructure.Persistence;

/// <summary>
/// Adaptador concreto de <see cref="IUnitOfWork"/> sobre o <see cref="RegistryDbContext"/>.
/// Tipo concreto para o codegen do Wolverine resolver sem service location.
/// </summary>
public sealed class RegistryUnitOfWork(RegistryDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
