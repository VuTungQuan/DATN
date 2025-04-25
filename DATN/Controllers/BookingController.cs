using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DATN.DTO;
using DATN.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Newtonsoft.Json;
using System.Security.Claims;

namespace DATN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // GET: api/Bookings
        [HttpGet]
        public async Task<ActionResult<PaginatedResponseDTO<BookingDTO>>> GetBookings(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int? pitchId = null,
            [FromQuery] int? userId = null,
            [FromQuery] string? status = null)
        {
            try
            {
                var searchDto = new BookingSearchDTO
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    PitchID = pitchId,
                    UserID = userId,
                    Status = status
                };

                var result = await _bookingService.GetPaginatedBookingsAsync(pageNumber, pageSize, searchDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/Bookings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDTO<BookingDTO>>> GetBooking(int id)
        {
            var result = await _bookingService.GetBookingByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        // GET: api/Bookings/user/5
        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<ResponseDTO<List<BookingDTO>>>> GetBookingsByUser(int userId)
        {
            // Kiểm tra người dùng hiện tại có quyền xem không
            if (!User.IsInRole("Admin"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId) || currentUserId != userId)
                {
                    return Forbid();
                }
            }
            
            var result = await _bookingService.GetBookingsByUserIdAsync(userId);
            return Ok(result);
        }

        // GET: api/Bookings/current-user
        [HttpGet("current-user")]
        [Authorize]
        public async Task<ActionResult<ResponseDTO<List<BookingDTO>>>> GetCurrentUserBookings()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { success = false, message = "Không thể xác định người dùng" });
            }
            
            var result = await _bookingService.GetBookingsByUserIdAsync(userId);
            return Ok(result);
        }

        // GET: api/Bookings/available
        [HttpGet("available")]
        public async Task<ActionResult<ResponseDTO<List<AvailablePitchDTO>>>> GetAvailablePitches(
            [FromQuery] DateTime date,
            [FromQuery] string startTime,
            [FromQuery] string endTime,
            [FromQuery] int? pitchTypeId = null)
        {
            try
            {
                if (!TimeSpan.TryParse(startTime, out var start) || !TimeSpan.TryParse(endTime, out var end))
                {
                    return BadRequest(new { success = false, message = "Thời gian không hợp lệ" });
                }

                var result = await _bookingService.GetAvailablePitchesAsync(date, start, end, pitchTypeId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/Bookings
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ResponseDTO<BookingDTO>>> CreateBooking([FromBody] BookingRequestDTO request)
        {
            try
            {
                // Lấy UserId từ token JWT
                var userId = 0;
                
                // Sử dụng ClaimTypes.NameIdentifier hoặc "uid" claim dựa trên kết quả test
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    userIdClaim = User.FindFirst("uid")?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out userId))
                {
                    var allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
                    Console.WriteLine("Tất cả claims: " + JsonConvert.SerializeObject(allClaims));
                    
                    return BadRequest(new { success = false, message = "Không thể xác định người dùng", claims = allClaims });
                }

                var result = await _bookingService.CreateBookingAsync(userId, request.Booking);
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // PUT: api/Bookings/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ResponseDTO<BookingDTO>>> UpdateBooking(int id, [FromBody] BookingUpdateDTO bookingDto)
        {
            var result = await _bookingService.UpdateBookingAsync(id, bookingDto);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        // PUT: api/Bookings/5/cancel
        [HttpPut("{id}/cancel")]
        [Authorize]
        public async Task<ActionResult<ResponseDTO<bool>>> CancelBooking(int id)
        {
            var result = await _bookingService.CancelBookingAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        // PUT: api/Bookings/5/confirm
        [HttpPut("{id}/confirm")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseDTO<bool>>> ConfirmBooking(int id)
        {
            var result = await _bookingService.ConfirmBookingAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        // PUT: api/Bookings/5/complete
        [HttpPut("{id}/complete")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseDTO<bool>>> CompleteBooking(int id)
        {
            var result = await _bookingService.CompleteBookingAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        // GET: api/Bookings/stats/status
        [HttpGet("stats/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseDTO<Dictionary<string, int>>>> GetBookingStatsByStatus()
        {
            var result = await _bookingService.GetBookingStatsByStatusAsync();
            return Ok(result);
        }

        // GET: api/Bookings/stats/date-range
        [HttpGet("stats/date-range")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseDTO<Dictionary<DateTime, int>>>> GetBookingStatsByDateRange(
            [FromQuery] DateTime fromDate, 
            [FromQuery] DateTime toDate)
        {
            var result = await _bookingService.GetBookingStatsByDateRangeAsync(fromDate, toDate);
            return Ok(result);
        }

        // GET: api/Booking/booked-slots
        [HttpGet("booked-slots")]
        public async Task<ActionResult<ResponseDTO<List<BookedTimeSlotDTO>>>> GetBookedTimeSlots(
            [FromQuery] int pitchId,
            [FromQuery] DateTime date)
        {
            try
            {
                var result = await _bookingService.GetBookedTimeSlotsAsync(pitchId, date);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/Booking/check-availability
        [HttpGet("check-availability")]
        public async Task<ActionResult<ResponseDTO<bool>>> CheckPitchAvailability(
            [FromQuery] int pitchId,
            [FromQuery] DateTime date,
            [FromQuery] string startTime,
            [FromQuery] string endTime)
        {
            try
            {
                if (!TimeSpan.TryParse(startTime, out var start) || !TimeSpan.TryParse(endTime, out var end))
                {
                    return BadRequest(new { success = false, message = "Thời gian không hợp lệ" });
                }

                var result = await _bookingService.CheckPitchAvailabilityAsync(pitchId, date, start, end);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
} 