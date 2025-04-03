using DATN.Model;

namespace DATN.Repositories
{
    public interface IBookingRepository
    {
        Task<IEnumerable<Booking>> GetAllBookingsAsync();
        Task<Booking?> GetBookingByIdAsync(int id);
        Task AddBookingAsync(Booking booking);
        Task UpdateBookingAsync(Booking booking);
        Task DeleteBookingAsync(int id);
        Task<bool> IsPitchAvailableAsync(int pitchId, DateTime bookingDate, TimeSpan startTime, TimeSpan endTime);
        
        Task<decimal> GetRevenueAsync(DateTime startDate, DateTime endDate);
        Task<int> GetBookingsCountAsync(DateTime startDate, DateTime endDate);
    }
}
