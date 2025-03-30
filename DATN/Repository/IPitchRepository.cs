using DATN.Model;

namespace DATN.Repositories
{
    public interface IPitchRepository
    {
        Task<IEnumerable<Pitch>> GetAllPitchesAsync();
        Task<Pitch?> GetPitchByIdAsync(int id);
        Task AddPitchAsync(Pitch pitch);
        Task UpdatePitchAsync(Pitch pitch);
        Task DeletePitchAsync(int id);
    }
}
