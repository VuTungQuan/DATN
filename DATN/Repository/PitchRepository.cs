using DATN.Data;
using DATN.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN.Repositories
{
    public class PitchRepository : IPitchRepository
    {
        private readonly OderPitchDbContext _context;

        public PitchRepository(OderPitchDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Pitch>> GetAllPitchesAsync()
        {
            return await _context.Pitches.ToListAsync();
        }

        public async Task<Pitch?> GetPitchByIdAsync(int id)
        {
            return await _context.Pitches.FindAsync(id);
        }

        public async Task AddPitchAsync(Pitch pitch)
        {
            _context.Pitches.Add(pitch);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePitchAsync(Pitch pitch)
        {
            _context.Entry(pitch).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeletePitchAsync(int id)
        {
            var pitch = await _context.Pitches.FindAsync(id);
            if (pitch != null)
            {
                _context.Pitches.Remove(pitch);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<Pitch?> GetPitchByNameAsync(string name)
        {
            return await _context.Pitches.FirstOrDefaultAsync(p => p.Name == name);
        }

        public async Task<IEnumerable<Pitch>> GetPitchesByPitchTypeIDAsync(int typeId)
        {
            return await _context.Pitches.Where(p => p.PitchTypeID == typeId).ToListAsync();
        }

    }
}
