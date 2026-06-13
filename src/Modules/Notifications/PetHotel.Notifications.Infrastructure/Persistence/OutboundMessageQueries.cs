using Microsoft.EntityFrameworkCore;
using PetHotel.Notifications.Application.Abstractions;
using PetHotel.Notifications.Application.Reports;
using PetHotel.Notifications.Domain.Reports;

namespace PetHotel.Notifications.Infrastructure.Persistence;

/// <summary>Lado de leitura dos relatórios (AsNoTracking + query filter de tenant, docs/04).</summary>
public sealed class OutboundMessageQueries(NotificationsDbContext dbContext) : IOutboundMessageQueries
{
    public async Task<IReadOnlyList<OutboundMessageDto>> GetByTutorAsync(
        Guid tutorId, CancellationToken cancellationToken = default)
    {
        var tutor = new TutorReference(tutorId);
        var rows = await dbContext.OutboundMessages
            .AsNoTracking()
            .Where(m => m.Tutor == tutor)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<OutboundMessageDto>> GetByReservationAsync(
        Guid reservationId, CancellationToken cancellationToken = default)
    {
        var rows = await dbContext.OutboundMessages
            .AsNoTracking()
            .Where(m => m.ReservationId == reservationId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    private static OutboundMessageDto ToDto(OutboundMessage m) =>
        new(m.Id.Value, m.Tutor.Value, m.Pet.Value, m.ReservationId, m.ReportDate, m.Title, m.Content,
            m.Status.ToString(), m.SentAt, m.CreatedBy, m.CreatedAt);
}
