using Microsoft.AspNetCore.Mvc;
using DATN.Repositories;
using DATN.Model;
using Microsoft.AspNetCore.Authorization;

namespace DATN.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentRepository _paymentRepository;

        public PaymentsController(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPayments()
        {
            return Ok(await _paymentRepository.GetAllPaymentsAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var payment = await _paymentRepository.GetPaymentByIdAsync(id);
            if (payment == null) return NotFound();
            return Ok(payment);
        }

        [HttpPost]
        public async Task<IActionResult> AddPayment([FromBody] Payment payment)
        {
            if (payment == null) return BadRequest();
            await _paymentRepository.AddPaymentAsync(payment);
            return CreatedAtAction(nameof(GetPayment), new { id = payment.PaymentID }, payment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] Payment payment)
        {
            if (id != payment.PaymentID) return BadRequest();
            await _paymentRepository.UpdatePaymentAsync(payment);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            await _paymentRepository.DeletePaymentAsync(id);
            return NoContent();
        }
    }
}
