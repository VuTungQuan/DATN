using DATN.Model;
using Microsoft.AspNetCore.Mvc;

namespace DATN.Repositories
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<Payment>> GetAllPaymentsAsync();
        Task<Payment?> GetPaymentByIdAsync(int id);
        Task AddPaymentAsync(Payment payment);
        Task UpdatePaymentAsync(Payment payment);
        Task DeletePaymentAsync(int id);
        Task<Payment?> GetPaymentByBookingIdAsync(int bookingId);
        Task<IActionResult> CompletePayment(int bookingId, string transactionId);
    }
}
