using PetHotel.Notifications.Application.Reports;

namespace PetHotel.Notifications.Application.Abstractions;

/// <summary>Porta de leitura dos relatórios (AsNoTracking + query filter de tenant, docs/04).</summary>
public interface IOutboundMessageQueries
{
    /// <summary>Relatórios de um tutor, mais recentes primeiro.</summary>
    Task<IReadOnlyList<OutboundMessageDto>> GetByTutorAsync(Guid tutorId, CancellationToken cancellationToken = default);

    /// <summary>Relatórios de uma estadia, mais recentes primeiro.</summary>
    Task<IReadOnlyList<OutboundMessageDto>> GetByReservationAsync(Guid reservationId, CancellationToken cancellationToken = default);
}
