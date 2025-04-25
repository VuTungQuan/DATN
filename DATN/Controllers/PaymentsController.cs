using Microsoft.AspNetCore.Mvc;
using DATN.Repository;
using DATN.Model;
using DATN.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult<PaginatedResponseDTO<PaymentDTO>>> GetPayments(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] string? paymentMethod = null,
            [FromQuery] int? bookingId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var payments = await _paymentRepository.GetAllPaymentsAsync();
                
                // Áp dụng các bộ lọc
                if (!string.IsNullOrEmpty(status))
                {
                    payments = payments.Where(p => p.PaymentStatus.Equals(status, StringComparison.OrdinalIgnoreCase));
                }
                
                if (!string.IsNullOrEmpty(paymentMethod))
                {
                    payments = payments.Where(p => p.PaymentMethod.Equals(paymentMethod, StringComparison.OrdinalIgnoreCase));
                }
                
                if (bookingId.HasValue)
                {
                    payments = payments.Where(p => p.BookingID == bookingId.Value);
                }
                
                if (fromDate.HasValue)
                {
                    payments = payments.Where(p => p.PaidDate >= fromDate.Value);
                }
                
                if (toDate.HasValue)
                {
                    // Đảm bảo toDate bao gồm cả giờ cuối cùng của ngày
                    var endDate = toDate.Value.Date.AddDays(1).AddTicks(-1);
                    payments = payments.Where(p => p.PaidDate <= endDate);
                }
                
                // Đếm tổng số kết quả
                var totalItems = payments.Count();
                
                // Sắp xếp và phân trang
                var pagedPayments = payments
                    .OrderByDescending(p => p.PaidDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                // Chuyển đổi sang DTO
                var paymentDTOs = pagedPayments.Select(p => new PaymentDTO
                {
                    PaymentID = p.PaymentID,
                    BookingID = p.BookingID,
                    Amount = p.PaidAmount ?? 0,
                    PaymentMethod = p.PaymentMethod,
                    Status = p.PaymentStatus,
                    TransactionID = p.TransactionID,
                    PaymentDate = p.PaidDate ?? DateTime.Now
                }).ToList();
                
                // Sử dụng PaginatedResponseDTO
                var result = PaginatedResponseDTO<PaymentDTO>.Create(
                    paymentDTOs,
                    totalItems,
                    pageNumber,
                    pageSize,
                    "Lấy danh sách thanh toán thành công"
                );
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<List<PaymentDTO>>
                {
                    Success = false,
                    Message = $"Lỗi khi lấy danh sách thanh toán: {ex.Message}"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDTO<PaymentDTO>>> GetPayment(int id)
        {
            try
            {
                var payment = await _paymentRepository.GetPaymentByIdAsync(id);
                if (payment == null)
                {
                    return NotFound(new ResponseDTO<PaymentDTO>
                    {
                        Success = false,
                        Message = "Không tìm thấy thanh toán"
                    });
                }
                
                var paymentDTO = new PaymentDTO
                {
                    PaymentID = payment.PaymentID,
                    BookingID = payment.BookingID,
                    Amount = payment.PaidAmount ?? 0,
                    PaymentMethod = payment.PaymentMethod,
                    Status = payment.PaymentStatus,
                    TransactionID = payment.TransactionID,
                    PaymentDate = payment.PaidDate ?? DateTime.Now
                };
                
                return Ok(new ResponseDTO<PaymentDTO>
                {
                    Success = true,
                    Message = "Lấy thông tin thanh toán thành công",
                    Data = paymentDTO
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<PaymentDTO>
                {
                    Success = false,
                    Message = $"Lỗi khi lấy thông tin thanh toán: {ex.Message}"
                });
            }
        }

        [HttpGet("booking/{bookingId}")]
        public async Task<ActionResult<ResponseDTO<PaymentDTO>>> GetPaymentByBookingId(int bookingId)
        {
            try
            {
                var payment = await _paymentRepository.GetPaymentByBookingIdAsync(bookingId);
                if (payment == null)
                {
                    return NotFound(new ResponseDTO<PaymentDTO>
                    {
                        Success = false,
                        Message = "Không tìm thấy thanh toán cho đặt sân này"
                    });
                }
                
                var paymentDTO = new PaymentDTO
                {
                    PaymentID = payment.PaymentID,
                    BookingID = payment.BookingID,
                    Amount = payment.PaidAmount ?? 0,
                    PaymentMethod = payment.PaymentMethod,
                    Status = payment.PaymentStatus,
                    TransactionID = payment.TransactionID,
                    PaymentDate = payment.PaidDate ?? DateTime.Now
                };
                
                return Ok(new ResponseDTO<PaymentDTO>
                {
                    Success = true,
                    Message = "Lấy thông tin thanh toán thành công",
                    Data = paymentDTO
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<PaymentDTO>
                {
                    Success = false,
                    Message = $"Lỗi khi lấy thông tin thanh toán: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddPayment([FromBody] PaymentCreateDTO paymentDTO)
        {
            if (paymentDTO == null) return BadRequest(new ResponseDTO<PaymentDTO>
            {
                Success = false,
                Message = "Dữ liệu không hợp lệ"
            });
            
            try
            {
                var payment = new Payment
                {
                    BookingID = paymentDTO.BookingID,
                    PaymentMethod = paymentDTO.PaymentMethod,
                    PaymentStatus = "Paid",
                    PaidAmount = paymentDTO.Amount,
                    PaidDate = DateTime.Now,
                    TransactionID = paymentDTO.TransactionID
                };
                
                await _paymentRepository.AddPaymentAsync(payment);
                
                var createdPaymentDTO = new PaymentDTO
                {
                    PaymentID = payment.PaymentID,
                    BookingID = payment.BookingID,
                    Amount = payment.PaidAmount ?? 0,
                    PaymentMethod = payment.PaymentMethod,
                    Status = payment.PaymentStatus,
                    TransactionID = payment.TransactionID,
                    PaymentDate = payment.PaidDate ?? DateTime.Now
                };
                
                return CreatedAtAction(nameof(GetPayment), new { id = payment.PaymentID }, new ResponseDTO<PaymentDTO>
                {
                    Success = true,
                    Message = "Tạo thanh toán thành công",
                    Data = createdPaymentDTO
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<PaymentDTO>
                {
                    Success = false,
                    Message = $"Lỗi khi tạo thanh toán: {ex.Message}"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] PaymentUpdateDTO paymentDTO)
        {
            try
            {
                var payment = await _paymentRepository.GetPaymentByIdAsync(id);
                if (payment == null)
                {
                    return NotFound(new ResponseDTO<bool>
                    {
                        Success = false,
                        Message = "Không tìm thấy thanh toán"
                    });
                }
                
                // Cập nhật các trường nếu có
                if (paymentDTO.Amount.HasValue)
                {
                    payment.PaidAmount = paymentDTO.Amount.Value;
                }
                
                if (!string.IsNullOrEmpty(paymentDTO.PaymentMethod))
                {
                    payment.PaymentMethod = paymentDTO.PaymentMethod;
                }
                
                if (!string.IsNullOrEmpty(paymentDTO.Status))
                {
                    payment.PaymentStatus = paymentDTO.Status;
                }
                
                if (!string.IsNullOrEmpty(paymentDTO.TransactionID))
                {
                    payment.TransactionID = paymentDTO.TransactionID;
                }
                
                await _paymentRepository.UpdatePaymentAsync(payment);
                
                return Ok(new ResponseDTO<bool>
                {
                    Success = true,
                    Message = "Cập nhật thanh toán thành công",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<bool>
                {
                    Success = false,
                    Message = $"Lỗi khi cập nhật thanh toán: {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            try
            {
                var payment = await _paymentRepository.GetPaymentByIdAsync(id);
                if (payment == null)
                {
                    return NotFound(new ResponseDTO<bool>
                    {
                        Success = false,
                        Message = "Không tìm thấy thanh toán"
                    });
                }
                
                await _paymentRepository.DeletePaymentAsync(id);
                
                return Ok(new ResponseDTO<bool>
                {
                    Success = true,
                    Message = "Xóa thanh toán thành công",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<bool>
                {
                    Success = false,
                    Message = $"Lỗi khi xóa thanh toán: {ex.Message}"
                });
            }
        }
    }
}
