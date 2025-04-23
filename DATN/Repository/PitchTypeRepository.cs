using DATN.Data;
using DATN.Model;
using Microsoft.EntityFrameworkCore;

namespace DATN.Repositories
{
    public class PitchTypeRepository : IPitchTypeRepository
    {
        private readonly OderPitchDbContext _context;

        public PitchTypeRepository(OderPitchDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PitchType>> GetAllPitchTypesAsync()
        {
            return await _context.PitchTypes.ToListAsync();
        }

        public async Task<PitchType> GetPitchTypeByIdAsync(int id)
        {
            return await _context.PitchTypes.FindAsync(id);
        }
        public async Task<PitchType> GetPitchTypeByNameAsync(string name)
        {
            return await _context.PitchTypes.FirstOrDefaultAsync(u => u.Name == name);
        }
        public async Task AddPitchTypeAsync(PitchType pitchType)
        {
            pitchType.PitchTypeID = 0;
            _context.PitchTypes.Add(pitchType);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePitchTypeAsync(PitchType pitchType)
        {

            // Cập nhật các trường thông tin khác của loại sân
            _context.Entry(pitchType).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeletePitchTypeAsync(int id)
        {
            var pitchType = await _context.PitchTypes.FindAsync(id);
            if (pitchType != null)
            {
                _context.PitchTypes.Remove(pitchType);
                await _context.SaveChangesAsync();
            }
        }

    }
}
