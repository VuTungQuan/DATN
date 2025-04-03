using Microsoft.AspNetCore.Mvc;
using DATN.Model;
using DATN.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountController : ControllerBase
    {
        private readonly IDiscountRepository _discountRepository;

        public DiscountController(IDiscountRepository discountRepository)
        {
            _discountRepository = discountRepository;
        }

        // Lấy tất cả discount
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Discount>>> GetDiscounts()
        {
            var discounts = await _discountRepository.GetAllDiscountsAsync();
            return Ok(discounts);
        }

        // Lấy discount theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Discount>> GetDiscount(int id)
        {
            var discount = await _discountRepository.GetDiscountByIdAsync(id);
            if (discount == null)
            {
                return NotFound();
            }
            return Ok(discount);
        }

        // Thêm discount mới
        [HttpPost]
        public async Task<ActionResult<Discount>> PostDiscount(Discount discount)
        {
            await _discountRepository.AddDiscountAsync(discount);
            return CreatedAtAction(nameof(GetDiscount), new { id = discount.DiscountID }, discount);
        }

        // Cập nhật discount
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDiscount(int id, Discount discount)
        {
            if (id != discount.DiscountID)
            {
                return BadRequest();
            }

            await _discountRepository.UpdateDiscountAsync(discount);
            return NoContent();
        }

        // Xóa discount
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDiscount(int id)
        {
            await _discountRepository.DeleteDiscountAsync(id);
            return NoContent();
        }
    }
}
