using Microsoft.EntityFrameworkCore;
using PetHotel.Booking.Application.Abstractions;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.Infrastructure.Persistence;

/// <summary>
/// Unit of Work do Booking. Traduz o conflito de xmin (<see cref="DbUpdateConcurrencyException"/>)
/// em <see cref="ConcurrencyConflictException"/> para o handler devolver Conflict (docs/04).
/// </summary>
public sealed class BookingUnitOfWork(BookingDbContext dbContext) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException("Conflito de concorrência otimista (xmin).", ex);
        }
    }
}
