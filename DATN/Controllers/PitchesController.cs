using DATN.Model;
using DATN.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

                pitch.ImageUrl = $"/images/{fileName}";
            }

            
            pitch.PitchID = 0; 

            await _pitchRepository.AddPitchAsync(pitch);
            return CreatedAtAction(nameof(GetPitch), new { id = pitch.PitchID }, pitch);
        }




        [HttpPut("{id}")]
        public async Task<IActionResult> PutPitch(int id, [FromBody] Pitch pitch, IFormFile imageUrl)
        {
            if (id != pitch.PitchID)
            {
                return BadRequest("ID không hợp lệ.");
            }

            // Kiểm tra nếu có ảnh mới
            if (imageUrl != null && imageUrl.Length > 0)
            {
                var imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images","pitch");
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

                pitch.ImageUrl = $"/images/{fileName}";  // Lưu lại đường dẫn hình ảnh
            }

            await _pitchRepository.UpdatePitchAsync(pitch);
            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePitch(int id)
        {
            await _pitchRepository.DeletePitchAsync(id);
            return NoContent();
        }
    }
}
