using Microsoft.EntityFrameworkCore;
using DATN.Data;
using DATN.Model;

namespace DATN.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly OderPitchDbContext _dbContext; // Renamed to avoid ambiguity

        public PaymentRepository(OderPitchDbContext dbContext) // Updated parameter name
        {
            _dbContext = dbContext; // Updated assignment to match the renamed field
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            return await _dbContext.Payments.Include(p => p.Booking).ToListAsync();
        }

        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            return await _dbContext.Payments.Include(p => p.Booking).FirstOrDefaultAsync(p => p.PaymentID == id);
        }
        public async Task<Payment?> GetPaymentByBookingIdAsync(int bookingId)
        {
            return await _dbContext.Payments.Include(p => p.Booking).FirstOrDefaultAsync(p => p.BookingID == bookingId);
        }
        public async Task AddPaymentAsync(Payment payment)
        {
            _dbContext.Payments.Add(payment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdatePaymentAsync(Payment payment)
        {
            _dbContext.Payments.Update(payment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeletePaymentAsync(int id)
        {
            var payment = await _dbContext.Payments.FindAsync(id);
            if (payment != null)
            {
                _dbContext.Payments.Remove(payment);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
