using DATN.Model;
using DATN.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace DATN.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class pitchController : ControllerBase
    {
        private readonly IPitchRepository _pitchRepository;

        public pitchController(IPitchRepository pitchRepository)
        {
            _pitchRepository = pitchRepository;
        }

        // GET: api/pitch
        [HttpGet]
        public async Task<ActionResult> Getpitch([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var pitchs = await _pitchRepository.GetAllPitchesAsync(); // Lấy tất cả loại sân
            var pitchsList = pitchs.ToList(); // Chuyển đối tượng thành danh sách

            int totalCount = pitchsList.Count;
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize); // Tính tổng số trang

            var paginatedpitchs = pitchsList.Skip((page - 1) * pageSize).Take(pageSize); // Phân trang

            return Ok(new
            {
                items = paginatedpitchs,
                totalCount,
                totalPages
            });
        }

        // GET: api/pitch/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Pitch>> Getpitch(int id)
        {
            var pitch = await _pitchRepository.GetPitchByIdAsync(id);
            if (pitch == null)
            {
                return NotFound();
            }

            return Ok(pitch);
        }

        // POST: api/pitch
        [HttpPost]
        public async Task<ActionResult<Pitch>> Postpitch([FromForm] Pitch pitch, IFormFile imageUrl)
        {
            if (pitch == null)
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            // Kiểm tra nếu có hình ảnh, thì lưu hình ảnh vào thư mục
            if (imageUrl != null && imageUrl.Length > 0)
            {
                // Đảm bảo thư mục images tồn tại trong wwwroot
                var imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "UI", "images");
                if (!Directory.Exists(imageFolder))
                {
                    Directory.CreateDirectory(imageFolder);  // Tạo thư mục nếu chưa có
                }

                // Tạo tên tệp tin mới để tránh trùng lặp
                var fileName = Path.GetFileName(imageUrl.FileName); // Lấy tên tệp gốc từ client
                var filePath = Path.Combine(imageFolder, fileName); // Đường dẫn đầy đủ tới file

                // Lưu hình ảnh vào thư mục
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageUrl.CopyToAsync(stream); // Lưu hình ảnh vào thư mục
                }

                // Lưu đường dẫn vào cơ sở dữ liệu (image sẽ được truy cập từ wwwroot/images)
                pitch.ImageUrl = $"/images/{fileName}";
            }

            // Thêm loại sân vào cơ sở dữ liệu
            await _pitchRepository.AddPitchAsync(pitch);
            return CreatedAtAction(nameof(Getpitch), new { id = pitch.PitchID }, pitch);
        }

        // PUT: api/pitch/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Putpitch(int id, [FromForm] Pitch pitch, IFormFile imageUrl)
        {
            if (id != pitch.PitchID)
            {
                return BadRequest("ID không hợp lệ.");
            }

            if (imageUrl != null && imageUrl.Length > 0)
            {
                // Lưu hình ảnh nếu có ảnh mới
                var imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "UI", "images");
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

                pitch.ImageUrl = $"/images/{fileName}"; // Lưu lại đường dẫn hình ảnh
            }

            try
            {
                await _pitchRepository.UpdatePitchAsync(pitch);
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!pitchExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }



        // DELETE: api/pitch/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletepitch(int id)
        {
            await _pitchRepository.DeletePitchAsync(id);
            return NoContent();
        }

        private bool pitchExists(int id)
        {
            return _pitchRepository.GetPitchByIdAsync(id) != null;
        }
    }
}
