using PetHotel.Health.Application.Abstractions;

namespace PetHotel.Health.Infrastructure.Persistence;

/// <summary>
/// Adaptador concreto de <see cref="IUnitOfWork"/> sobre o <see cref="HealthDbContext"/>.
/// Tipo concreto para o codegen do Wolverine resolver sem service location.
/// </summary>
public sealed class HealthUnitOfWork(HealthDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
