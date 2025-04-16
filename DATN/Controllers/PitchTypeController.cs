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
    public class PitchTypeController : ControllerBase
    {
        private readonly IPitchTypeRepository _pitchTypeRepository;

        public PitchTypeController(IPitchTypeRepository pitchTypeRepository)
        {
            _pitchTypeRepository = pitchTypeRepository;
        }

        // GET: api/PitchType
        [HttpGet]
        public async Task<ActionResult> GetPitchType([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var pitchTypes = await _pitchTypeRepository.GetAllPitchTypesAsync(); // Lấy tất cả loại sân
            var pitchTypesList = pitchTypes.ToList(); // Chuyển đối tượng thành danh sách

            int totalCount = pitchTypesList.Count;
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize); // Tính tổng số trang

            var paginatedPitchTypes = pitchTypesList.Skip((page - 1) * pageSize).Take(pageSize); // Phân trang

            return Ok(new
            {
                items = paginatedPitchTypes,
                totalCount,
                totalPages
            });
        }

        // GET: api/PitchType/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PitchType>> GetPitchType(int id)
        {
            var pitchType = await _pitchTypeRepository.GetPitchTypeByIdAsync(id);
            if (pitchType == null)
            {
                return NotFound();
            }

            return Ok(pitchType);
        }

        // POST: api/PitchType
        [HttpPost]
        public async Task<ActionResult<PitchType>> PostPitchType([FromForm] PitchType pitchType, IFormFile imageUrl)
        {
            if (pitchType == null)
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
                pitchType.ImageUrl = $"/images/{fileName}";
            }

            // Thêm loại sân vào cơ sở dữ liệu
            await _pitchTypeRepository.AddPitchTypeAsync(pitchType);
            return CreatedAtAction(nameof(GetPitchType), new { id = pitchType.PitchTypeID }, pitchType);
        }

        // PUT: api/PitchType/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPitchType(int id, [FromForm] PitchType pitchType, IFormFile imageUrl)
        {
            if (id != pitchType.PitchTypeID)
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

                pitchType.ImageUrl = $"/images/{fileName}"; // Lưu lại đường dẫn hình ảnh
            }

            try
            {
                await _pitchTypeRepository.UpdatePitchTypeAsync(pitchType);
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PitchTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }



        // DELETE: api/PitchType/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePitchType(int id)
        {
            await _pitchTypeRepository.DeletePitchTypeAsync(id);
            return NoContent();
        }

        private bool PitchTypeExists(int id)
        {
            return _pitchTypeRepository.GetPitchTypeByIdAsync(id) != null;
        }
    }
}
