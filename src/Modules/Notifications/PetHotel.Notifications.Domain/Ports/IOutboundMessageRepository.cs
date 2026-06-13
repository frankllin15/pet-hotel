using PetHotel.Notifications.Domain.Reports;

namespace PetHotel.Notifications.Domain.Ports;

/// <summary>Porta de saída do agregado <see cref="OutboundMessage"/>.</summary>
public interface IOutboundMessageRepository
{
    Task<OutboundMessage?> FindAsync(OutboundMessageId id, CancellationToken cancellationToken = default);

    void Add(OutboundMessage message);
}
