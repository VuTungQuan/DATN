using Microsoft.EntityFrameworkCore;
using DATN.Data;

using DATN.Model;

namespace DATN.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly OderPitchDbContext _context;

        public BookingRepository(OderPitchDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
        {
            return await _context.Bookings.Include(b => b.Pitch).Include(b => b.User).ToListAsync();
        }

        public async Task<Booking?> GetBookingByIdAsync(int id)
        {
            return await _context.Bookings.Include(b => b.Pitch).Include(b => b.User).FirstOrDefaultAsync(b => b.BookingID == id);
        }

        public async Task AddBookingAsync(Booking booking)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBookingAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBookingAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsPitchAvailableAsync(int pitchId, DateTime bookingDate, TimeSpan startTime, TimeSpan endTime)
        {
            return !await _context.Bookings.AnyAsync(b =>
                b.PitchID == pitchId &&
                b.BookingDate == bookingDate &&
                ((b.StartTime < endTime && b.StartTime >= startTime) || (b.EndTime > startTime && b.EndTime <= endTime))
            );
        }
    }
}
