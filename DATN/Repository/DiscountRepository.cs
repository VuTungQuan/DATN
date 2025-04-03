using Microsoft.EntityFrameworkCore;
using DATN.Data;
using DATN.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly OderPitchDbContext _context;

        public DiscountRepository(OderPitchDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Discount>> GetAllDiscountsAsync()
        {
            return await _context.Discounts.ToListAsync();
        }

        public async Task<Discount?> GetDiscountByIdAsync(int id)
        {
            return await _context.Discounts.FirstOrDefaultAsync(d => d.DiscountID == id);
        }

        public async Task AddDiscountAsync(Discount discount)
        {
            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDiscountAsync(Discount discount)
        {
            _context.Discounts.Update(discount);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDiscountAsync(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount != null)
            {
                _context.Discounts.Remove(discount);
                await _context.SaveChangesAsync();
            }
        }
    }
}
