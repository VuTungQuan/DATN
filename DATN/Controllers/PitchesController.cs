using Microsoft.AspNetCore.Mvc;
using DATN.Repositories;
using DATN.Model;
using Microsoft.AspNetCore.Authorization;

namespace DATN.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PitchesController : ControllerBase
    {
        private readonly IPitchRepository _pitchRepository;

        public PitchesController(IPitchRepository pitchRepository)
        {
            _pitchRepository = pitchRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pitch>>> GetPitches()
        {
            return Ok(await _pitchRepository.GetAllPitchesAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Pitch>> GetPitch(int id)
        {
            var pitch = await _pitchRepository.GetPitchByIdAsync(id);
            if (pitch == null) return NotFound();
            return Ok(pitch);
        }

        [HttpPost]
        public async Task<IActionResult> PostPitch(Pitch pitch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _pitchRepository.AddPitchAsync(pitch);
                return CreatedAtAction("GetPitch", new { id = pitch.PitchID }, pitch);
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePitch(int id, [FromBody] Pitch pitch)
        {
            if (id != pitch.PitchID) return BadRequest();
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
