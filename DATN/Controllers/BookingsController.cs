using Microsoft.AspNetCore.Mvc;
using DATN.Repositories;
using DATN.Model;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DATN.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IPitchRepository _pitchRepository;

        public BookingsController(IBookingRepository bookingRepository, IPitchRepository pitchRepository)
        {
            _bookingRepository = bookingRepository;
            _pitchRepository = pitchRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
        {
            return Ok(await _bookingRepository.GetAllBookingsAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBooking(int id)
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(id);
            if (booking == null) return NotFound();
            return Ok(booking);
        }

        [HttpGet("CheckAvailability")]
        public async Task<IActionResult> CheckPitchAvailability(
            [FromQuery] int pitchId,
            [FromQuery] DateTime bookingDate,
            [FromQuery] TimeSpan startTime,
            [FromQuery] TimeSpan endTime)
        {
            var isAvailable = await _bookingRepository.IsPitchAvailableAsync(pitchId, bookingDate, startTime, endTime);
            return Ok(new { Available = isAvailable });
        }

        [HttpPost]
        public async Task<IActionResult> PostBooking(Booking booking)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the PitchID exists
            var pitch = await _pitchRepository.GetPitchByIdAsync(booking.PitchID);
            if (pitch == null)
            {
                return BadRequest("Invalid PitchID. The specified pitch does not exist.");
            }

            try
            {
                await _bookingRepository.AddBookingAsync(booking);
                return CreatedAtAction("GetBooking", new { id = booking.BookingID }, booking);
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] Booking booking)
        {
            if (id != booking.BookingID) return BadRequest();
            await _bookingRepository.UpdateBookingAsync(booking);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            await _bookingRepository.DeleteBookingAsync(id);
            return NoContent();
        }
    }
}
