using Microsoft.EntityFrameworkCore;
using PetHotel.Notifications.Domain.Ports;
using PetHotel.Notifications.Domain.Reports;

namespace PetHotel.Notifications.Infrastructure.Persistence.Repositories;

/// <summary>Repositório do agregado <see cref="OutboundMessage"/>.</summary>
public sealed class OutboundMessageRepository(NotificationsDbContext dbContext) : IOutboundMessageRepository
{
    public Task<OutboundMessage?> FindAsync(OutboundMessageId id, CancellationToken cancellationToken = default) =>
        dbContext.OutboundMessages.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public void Add(OutboundMessage message) => dbContext.OutboundMessages.Add(message);
}
