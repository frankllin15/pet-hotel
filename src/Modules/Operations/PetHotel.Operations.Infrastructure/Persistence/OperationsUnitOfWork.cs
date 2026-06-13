using PetHotel.Operations.Application.Abstractions;

namespace PetHotel.Operations.Infrastructure.Persistence;

/// <summary>
/// Adaptador concreto de <see cref="IUnitOfWork"/> sobre o <see cref="OperationsDbContext"/>.
/// Tipo concreto para o codegen do Wolverine resolver sem service location.
/// </summary>
public sealed class OperationsUnitOfWork(OperationsDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
