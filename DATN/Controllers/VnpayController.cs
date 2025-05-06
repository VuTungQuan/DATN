using DATN.Services;
using Microsoft.AspNetCore.Mvc;
using DATN.Helpers;
using System.Threading.Tasks;
using DATN.Model;
using DATN.Repository;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DATN.DTO;
using System.Linq;

namespace DATN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VnpayController : ControllerBase
    {
        private readonly IVnpayService _vnpayService;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IBookingService _bookingService;

        public VnpayController(
            IVnpayService vnpayService,
            IPaymentRepository paymentRepository,
            IBookingService bookingService)
        {
            _vnpayService = vnpayService;
            _paymentRepository = paymentRepository;
            _bookingService = bookingService;
        }

        [HttpGet("CreatePaymentUrl")]
        [Authorize]
        public async Task<ActionResult<ResponseDTO<string>>> CreatePaymentUrl(int bookingId)
        {
            try
            {
                // Log thông tin debugging
                Console.WriteLine($"CreatePaymentUrl: Bắt đầu xử lý cho booking ID {bookingId}");
                
                // Lấy thông tin booking
                var bookingResult = await _bookingService.GetBookingByIdAsync(bookingId);
                if (!bookingResult.Success || bookingResult.Data == null)
                {
                    Console.WriteLine($"CreatePaymentUrl: Không tìm thấy booking với ID {bookingId}");
                    return BadRequest(new ResponseDTO<string> { Success = false, Message = "Không tìm thấy thông tin đặt sân" });
                }

                var booking = bookingResult.Data;
                Console.WriteLine($"CreatePaymentUrl: Đã tìm thấy booking {bookingId}, trạng thái: {booking.Status}");

                // Kiểm tra trạng thái booking
                if (booking.Status != "Pending" && booking.Status != "Confirmed")
                {
                    Console.WriteLine($"CreatePaymentUrl: Booking không ở trạng thái có thể thanh toán: {booking.Status}");
                    return BadRequest(new ResponseDTO<string> { Success = false, Message = "Đặt sân không ở trạng thái có thể thanh toán" });
                }

                // Kiểm tra xem đã có thanh toán thành công chưa
                bool isAlreadyPaid = false;
                
                var existingPayment = await _paymentRepository.GetPaymentByBookingIdAsync(bookingId);
                if (existingPayment != null)
                {
                    Console.WriteLine($"CreatePaymentUrl: Đã tìm thấy thanh toán cho booking {bookingId}, trạng thái: {existingPayment.PaymentStatus}");
                    if (existingPayment.PaymentStatus == "Paid")
                    {
                        isAlreadyPaid = true;
                    }
                }
                else
                {
                    Console.WriteLine($"CreatePaymentUrl: Chưa có thanh toán cho booking {bookingId}");
                }

                if (isAlreadyPaid)
                {
                    return BadRequest(new ResponseDTO<string> { Success = false, Message = "Đặt sân này đã được thanh toán" });
                }

                // Lấy địa chỉ IP của người dùng
                var ipAddress = NetworkHelper.GetIpAddress(HttpContext);
                Console.WriteLine($"CreatePaymentUrl: IP của người dùng: {ipAddress}");

                var request = new PaymentRequestDTO
                {
                    PaymentId = DateTime.Now.Ticks,
                    Amount = (double)booking.TotalPrice,
                    Description = $"Thanh toán đặt sân {booking.PitchName} ngày {booking.BookingDate.ToString("dd/MM/yyyy")} từ {booking.StartTime.ToString(@"hh\:mm")} đến {booking.EndTime.ToString(@"hh\:mm")}",
                    IpAddress = ipAddress,
                    BookingId = bookingId
                };

                Console.WriteLine($"CreatePaymentUrl: Tạo URL thanh toán với số tiền {request.Amount}");
                try
                {
                    var paymentUrl = _vnpayService.CreatePaymentUrl(request);
                    Console.WriteLine($"CreatePaymentUrl: Đã tạo URL thanh toán thành công: {paymentUrl}");

                    // Lưu thông tin yêu cầu thanh toán
                    if (existingPayment == null)
                    {
                        Console.WriteLine($"CreatePaymentUrl: Tạo mới thanh toán cho booking {bookingId}");
                        var payment = new Payment
                        {
                            BookingID = bookingId,
                            PaymentMethod = "VNPAY",
                            PaymentStatus = "Pending",
                            PaidAmount = 0,
                            TransactionID = request.PaymentId.ToString()
                        };

                        await _paymentRepository.AddPaymentAsync(payment);
                        Console.WriteLine($"CreatePaymentUrl: Đã lưu thành công thanh toán mới với ID {payment.PaymentID}");
                    }
                    else
                    {
                        Console.WriteLine($"CreatePaymentUrl: Cập nhật thanh toán hiện có cho booking {bookingId}");
                        existingPayment.PaymentMethod = "VNPAY";
                        existingPayment.PaymentStatus = "Pending";
                        existingPayment.TransactionID = request.PaymentId.ToString();

                        await _paymentRepository.UpdatePaymentAsync(existingPayment);
                        Console.WriteLine($"CreatePaymentUrl: Đã cập nhật thành công thanh toán với ID {existingPayment.PaymentID}");
                    }

                    return Ok(new ResponseDTO<string> { Success = true, Data = paymentUrl, Message = "URL thanh toán đã được tạo" });
                }
                catch (Exception vnpayEx)
                {
                    Console.WriteLine($"CreatePaymentUrl: Lỗi khi tạo URL thanh toán VNPAY: {vnpayEx.Message}");
                    if (vnpayEx.InnerException != null)
                    {
                        Console.WriteLine($"CreatePaymentUrl: Inner Exception: {vnpayEx.InnerException.Message}");
                    }
                    throw new Exception("Không thể tạo đường dẫn thanh toán. Vui lòng vào mục Sân đã đặt để thanh toán sau.", vnpayEx);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreatePaymentUrl: Lỗi: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"CreatePaymentUrl: Inner Exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new ResponseDTO<string> { Success = false, Message = $"Lỗi khi tạo URL thanh toán: {ex.Message}" });
            }
        }

        [HttpGet("Callback")]
        public async Task<ActionResult> Callback()
        {
            try
            {
                if (Request.QueryString.HasValue)
                {
                    var result = _vnpayService.GetPaymentResult(Request.Query);
                    
                    // Lấy thông tin booking ID từ transaction description
                    string bookingIdString = Request.Query["vnp_OrderInfo"].ToString().Split('_').Last();
                    if (!int.TryParse(bookingIdString, out int bookingId))
                    {
                        // Nếu không thể lấy được bookingId từ OrderInfo, thử tìm payment bằng TransactionID
                        bookingId = await GetBookingIdFromTransactionId(result.PaymentId.ToString());
                        if (bookingId == 0)
                        {
                            return Redirect($"/payment-result.html?status=error&message=Không thể xác định thông tin đặt sân");
                        }
                    }

                    // Cập nhật thông tin thanh toán
                    var bookingResult = await _bookingService.GetBookingByIdAsync(bookingId);
                    if (!bookingResult.Success || bookingResult.Data == null)
                    {
                        return Redirect($"/payment-result.html?status=error&message=Không tìm thấy thông tin đặt sân");
                    }

                    var booking = bookingResult.Data;
                    
                    // Tìm payment tương ứng
                    var payment = await _paymentRepository.GetPaymentByBookingIdAsync(bookingId);
                    if (payment == null)
                    {
                        // Tạo mới payment nếu chưa có
                        payment = new Payment
                        {
                            BookingID = bookingId,
                            PaymentMethod = "VNPAY",
                            PaymentStatus = result.IsSuccess ? "Paid" : "Failed",
                            PaidAmount = result.IsSuccess ? booking.TotalPrice : 0,
                            PaidDate = result.IsSuccess ? DateTime.Now : null,
                            TransactionID = result.IsSuccess ? result.VnpayTransactionId.ToString() : result.PaymentId.ToString()
                        };

                        await _paymentRepository.AddPaymentAsync(payment);
                    }
                    else
                    {
                        // Cập nhật payment hiện có
                        payment.PaymentStatus = result.IsSuccess ? "Paid" : "Failed";
                        payment.PaidAmount = result.IsSuccess ? booking.TotalPrice : 0;
                        payment.PaidDate = result.IsSuccess ? DateTime.Now : null;
                        payment.TransactionID = result.IsSuccess ? result.VnpayTransactionId.ToString() : payment.TransactionID;

                        await _paymentRepository.UpdatePaymentAsync(payment);
                    }

                    // Lấy token hiện tại từ request
                    string token = string.Empty;
                    if (User.Identity.IsAuthenticated)
                    {
                        // Từ ClaimsPrincipal, lấy thông tin token hoặc sử dụng userID để tạo token mới
                        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                        {
                            // Nếu booking có thông tin userId khớp với người dùng đang đăng nhập
                            if (booking.UserID == userId)
                            {
                                token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                            }
                        }
                    }

                    // Nếu thanh toán thành công, cập nhật trạng thái booking
                    if (result.IsSuccess)
                    {
                        var updateBookingDto = new BookingUpdateDTO { Status = "Confirmed" };
                        await _bookingService.UpdateBookingAsync(bookingId, updateBookingDto);

                        // Chuyển hướng đến trang kết quả thanh toán thành công
                        var redirectUrl = $"/payment-result.html?status=success&bookingId={bookingId}";
                        if (!string.IsNullOrEmpty(token))
                        {
                            redirectUrl += $"&token={token}";
                        }
                        return Redirect(redirectUrl);
                    }
                    else
                    {
                        // Chuyển hướng đến trang kết quả thanh toán thất bại
                        var redirectUrl = $"/payment-result.html?status=failed&message={result.TransactionStatus.Description}";
                        if (!string.IsNullOrEmpty(token))
                        {
                            redirectUrl += $"&token={token}";
                        }
                        return Redirect(redirectUrl);
                    }
                }

                return Redirect("/payment-result.html?status=error&message=Không có thông tin thanh toán");
            }
            catch (Exception ex)
            {
                return Redirect($"/payment-result.html?status=error&message=Lỗi xử lý thanh toán: {ex.Message}");
            }
        }

        [HttpGet("IpnAction")]
        public async Task<IActionResult> IpnAction()
        {
            try
            {
                if (Request.QueryString.HasValue)
                {
                    var result = _vnpayService.GetPaymentResult(Request.Query);
                    
                    // Kiểm tra kết quả thanh toán
                    if (result.IsSuccess)
                    {
                        // Lấy thông tin booking ID từ transaction description
                        string bookingIdString = Request.Query["vnp_OrderInfo"].ToString().Split('_').Last();
                        if (!int.TryParse(bookingIdString, out int bookingId))
                        {
                            // Nếu không thể lấy được bookingId từ OrderInfo, thử tìm payment bằng TransactionID
                            bookingId = await GetBookingIdFromTransactionId(result.PaymentId.ToString());
                            if (bookingId == 0)
                            {
                                return Ok(); // IPN yêu cầu trả về HTTP 200 ngay cả khi xử lý thất bại
                            }
                        }

                        // Cập nhật thông tin thanh toán
                        var bookingResult = await _bookingService.GetBookingByIdAsync(bookingId);
                        if (!bookingResult.Success || bookingResult.Data == null)
                        {
                            return Ok();
                        }

                        var booking = bookingResult.Data;
                        
                        // Tìm payment tương ứng
                        var payment = await _paymentRepository.GetPaymentByBookingIdAsync(bookingId);
                        if (payment == null)
                        {
                            // Tạo mới payment nếu chưa có
                            payment = new Payment
                            {
                                BookingID = bookingId,
                                PaymentMethod = "VNPAY",
                                PaymentStatus = "Paid",
                                PaidAmount = booking.TotalPrice,
                                PaidDate = DateTime.Now,
                                TransactionID = result.VnpayTransactionId.ToString()
                            };

                            await _paymentRepository.AddPaymentAsync(payment);
                        }
                        else if (payment.PaymentStatus != "Paid")
                        {
                            // Chỉ cập nhật payment nếu chưa thanh toán thành công
                            payment.PaymentStatus = "Paid";
                            payment.PaidAmount = booking.TotalPrice;
                            payment.PaidDate = DateTime.Now;
                            payment.TransactionID = result.VnpayTransactionId.ToString();

                            await _paymentRepository.UpdatePaymentAsync(payment);
                        }
                        else
                        {
                            // Payment đã thanh toán thành công, không cần xử lý thêm
                            return Ok();
                        }

                        // Cập nhật trạng thái booking
                        var updateBookingDto = new BookingUpdateDTO { Status = "Confirmed" };
                        await _bookingService.UpdateBookingAsync(bookingId, updateBookingDto);
                    }

                    return Ok();
                }

                return Ok();
            }
            catch (Exception)
            {
                return Ok(); // IPN yêu cầu trả về HTTP 200 ngay cả khi xử lý thất bại
            }
        }

        private async Task<int> GetBookingIdFromTransactionId(string transactionId)
        {
            var payments = await _paymentRepository.GetAllPaymentsAsync();
            var payment = payments.FirstOrDefault(p => p.TransactionID == transactionId);
            return payment?.BookingID ?? 0;
        }
    }
} 