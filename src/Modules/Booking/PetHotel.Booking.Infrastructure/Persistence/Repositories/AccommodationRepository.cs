using Microsoft.EntityFrameworkCore;
using PetHotel.Booking.Domain.Accommodations;
using PetHotel.Booking.Domain.Ports;

namespace PetHotel.Booking.Infrastructure.Persistence.Repositories;

/// <summary>Repositório do agregado <see cref="Accommodation"/> (leituras filtradas por tenant).</summary>
public sealed class AccommodationRepository(BookingDbContext dbContext) : IAccommodationRepository
{
    public Task<Accommodation?> FindAsync(AccommodationId id, CancellationToken cancellationToken = default) =>
        dbContext.Accommodations.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public void Add(Accommodation accommodation) => dbContext.Accommodations.Add(accommodation);
}
