using Microsoft.EntityFrameworkCore;
using DATN.Data;
using DATN.Model;

namespace DATN.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly OderPitchDbContext _context;

        public PaymentRepository(OderPitchDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            return await _context.Payments.Include(p => p.Booking).ToListAsync();
        }

        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            return await _context.Payments.Include(p => p.Booking).FirstOrDefaultAsync(p => p.PaymentID == id);
        }

        public async Task AddPaymentAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePaymentAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePaymentAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
            }
        }
    }
}
