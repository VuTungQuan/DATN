using Microsoft.EntityFrameworkCore;
using DATN.Data;
using DATN.Model;
using Microsoft.AspNetCore.Mvc;

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

        public async Task<Payment?> GetPaymentByBookingIdAsync(int bookingId)
        {
            return await _context.Payments
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.BookingID == bookingId);
        }

        public async Task<IActionResult> CompletePayment(int bookingId, string transactionId)
        {
            var payment = await GetPaymentByBookingIdAsync(bookingId);
            if (payment == null)
            {
                // Trả về mã lỗi 404 nếu không tìm thấy thanh toán
                return new NotFoundObjectResult("Payment not found");
            }

            payment.PaymentStatus = "Paid";
            payment.TransactionID = transactionId;
            payment.PaidDate = DateTime.UtcNow;
            payment.PaidAmount = payment.Booking.TotalPrice;

            await UpdatePaymentAsync(payment);

            // Trả về mã thành công 200 OK và đối tượng thanh toán
            return new OkObjectResult(payment);
        }
    }
}
