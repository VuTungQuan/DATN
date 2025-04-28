using DATN.Model;
using DATN.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        
        // Endpoint để trả về các khung giờ mặc định cho từng sân
        [HttpGet("{id}/time-slots")]
        public async Task<ActionResult> GetTimeSlots(int id)
        {
            try
            {
                // Lấy thông tin sân
                var pitch = await _pitchRepository.GetPitchByIdAsync(id);
                if (pitch == null)
                {
                    return NotFound("Không tìm thấy sân bóng.");
                }

                // Tạo cấu hình khung giờ mặc định dựa trên giá của sân
                decimal basePrice = pitch.Price;
                decimal morningPrice = basePrice;
                decimal afternoonPrice = basePrice;
                decimal eveningPrice = Math.Round(basePrice * 1.2m); // Giá buổi tối cao hơn 20%

                var timeSlots = new List<object>
                {
                    // Buổi sáng
                    new { startTime = "06:00:00", endTime = "07:30:00", price = morningPrice, period = "Sáng" },
                    new { startTime = "07:30:00", endTime = "09:00:00", price = morningPrice, period = "Sáng" },
                    new { startTime = "09:00:00", endTime = "10:30:00", price = morningPrice, period = "Sáng" },
                    
                    // Buổi chiều
                    new { startTime = "13:00:00", endTime = "14:30:00", price = afternoonPrice, period = "Chiều" },
                    new { startTime = "14:30:00", endTime = "16:00:00", price = afternoonPrice, period = "Chiều" },
                    new { startTime = "16:00:00", endTime = "17:30:00", price = afternoonPrice, period = "Chiều" },
                    
                    // Buổi tối
                    new { startTime = "18:00:00", endTime = "19:30:00", price = eveningPrice, period = "Tối" },
                    new { startTime = "19:30:00", endTime = "21:00:00", price = eveningPrice, period = "Tối" },
                    new { startTime = "21:00:00", endTime = "22:30:00", price = eveningPrice, period = "Tối" }
                };

                return Ok(timeSlots);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
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
            var task = _pitchRepository.GetPitchByIdAsync(id);
            task.Wait();
            return task.Result != null;
        }

        [HttpGet("check-combined-parent")]
        public async Task<IActionResult> CheckCombinedParent([FromQuery] int pitchId)
        {
            try
            {
                // Lấy tất cả các sân
                var allPitches = await _pitchRepository.GetAllPitchesAsync();

                // Tìm các sân gộp chứa pitchId
                var combinedPitches = allPitches.Where(p => 
                    !string.IsNullOrEmpty(p.Description) && 
                    p.Description.StartsWith("CombinedFrom:") && 
                    p.Description.Replace("CombinedFrom:", "")
                        .Split(',')
                        .Any(id => int.TryParse(id, out int componentId) && componentId == pitchId)
                ).ToList();

                if (combinedPitches.Any())
                {
                    // Có sân gộp chứa sân này
                    return Ok(new { 
                        isPartOfCombined = true, 
                        combinedPitchCount = combinedPitches.Count,
                        combinedPitchIds = combinedPitches.Select(p => p.PitchID),
                        combinedPitchName = string.Join(", ", combinedPitches.Select(p => p.Name))
                    });
                }
                else
                {
                    // Không phải là thành phần của sân gộp nào
                    return Ok(new { isPartOfCombined = false });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
} 