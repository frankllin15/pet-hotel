using PetHotel.Notifications.Application.Abstractions;

namespace PetHotel.Notifications.Infrastructure.Persistence;

/// <summary>Adaptador concreto de <see cref="IUnitOfWork"/> sobre o <see cref="NotificationsDbContext"/>.</summary>
public sealed class NotificationsUnitOfWork(NotificationsDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
