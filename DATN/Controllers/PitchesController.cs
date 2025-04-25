using DATN.Model;
using DATN.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class PitchController : ControllerBase
    {
        private readonly IPitchRepository _pitchRepository;

        public PitchController(IPitchRepository pitchRepository)
        {
            _pitchRepository = pitchRepository;
        }

        
        [HttpGet]
        public async Task<ActionResult> GetPitches([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var pitches = await _pitchRepository.GetAllPitchesAsync();
            var pitchList = pitches.ToList();

            int totalCount = pitchList.Count;
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var paginatedPitches = pitchList.Skip((page - 1) * pageSize).Take(pageSize);

            return Ok(new
            {
                items = paginatedPitches,
                totalCount,
                totalPages
            });
        }

        
        [HttpGet("{id}")]
        public async Task<ActionResult<Pitch>> GetPitch(int id)
        {
            var pitch = await _pitchRepository.GetPitchByIdAsync(id);
            if (pitch == null)
            {
                return NotFound();

            }
            return Ok(pitch);
        }

        [HttpGet("pitchtype/{id}")]
        public async Task<ActionResult<IEnumerable<Pitch>>> GetPitchesByPitchTypeID(int id)
        {
            var pitches = await _pitchRepository.GetPitchesByPitchTypeIDAsync(id);
            if (pitches == null || !pitches.Any())
            {
                return NotFound("Không tìm thấy sân bóng nào thuộc loại sân này.");
            }
            return Ok(pitches);
        }
        [HttpPost]
        
        public async Task<ActionResult<Pitch>> PostPitch([FromForm] Pitch pitch, IFormFile imageUrl)
        {
            if (pitch == null)
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            // Kiểm tra nếu có hình ảnh, thì lưu hình ảnh vào thư mục
            if (imageUrl != null && imageUrl.Length > 0)
            {
                var imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "pitch");
                if (!Directory.Exists(imageFolder))
                {
                    Directory.CreateDirectory(imageFolder);
                }

                var fileName = Path.GetFileName(imageUrl.FileName);
                var filePath = Path.Combine(imageFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageUrl.CopyToAsync(stream);
                }

                pitch.ImageUrl = $"/images/pitch/{fileName}";
            }

            // Reset các giá trị không cần thiết
            pitch.PitchID = 0;
            pitch.PitchType = null; // Không chèn thông tin PitchType mới
            pitch.Bookings = null; // Không chèn thông tin Bookings

            await _pitchRepository.AddPitchAsync(pitch);
            return CreatedAtAction(nameof(GetPitch), new { id = pitch.PitchID }, pitch);
        }




        [HttpPut("{id}")]
        public async Task<IActionResult> PutPitch(int id, [FromForm] Pitch pitch, IFormFile imageUrl)
        {
            if (pitch == null)
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            // Đảm bảo ID từ route được sử dụng
            pitch.PitchID = id;

            // Kiểm tra nếu có hình ảnh, thì lưu hình ảnh vào thư mục
            if (imageUrl != null && imageUrl.Length > 0)
            {
                var imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "pitch");
                if (!Directory.Exists(imageFolder))
                {
                    Directory.CreateDirectory(imageFolder);
                }

                var fileName = Path.GetFileName(imageUrl.FileName);
                var filePath = Path.Combine(imageFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageUrl.CopyToAsync(stream);
                }

                pitch.ImageUrl = $"/images/pitch/{fileName}";
            }

            // Reset các giá trị không cần thiết
            pitch.PitchType = null;
            pitch.Bookings = null;

            try
            {
                await _pitchRepository.UpdatePitchAsync(pitch);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PitchExists(id))
                {
                    return NotFound("Không tìm thấy sân bóng.");
                }
                else
                {
                    throw;
                }
            }
        }
        

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePitch(int id)
        {
            await _pitchRepository.DeletePitchAsync(id);
            return NoContent();
        }
        private bool PitchExists(int id)
        {
            return _pitchRepository.GetPitchByIdAsync(id) != null;
        }
    }
}
