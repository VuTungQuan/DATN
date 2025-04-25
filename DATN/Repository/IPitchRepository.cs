using DATN.Model;


namespace DATN.Repository
{
    public interface IPitchRepository
    {
        Task<IEnumerable<Pitch>> GetAllPitchesAsync();
        Task<Pitch?> GetPitchByIdAsync(int id);
        Task AddPitchAsync(Pitch pitch);
        Task UpdatePitchAsync(Pitch pitch);
        Task DeletePitchAsync(int id);
        Task<Pitch?> GetPitchByNameAsync(string name);
        Task<IEnumerable<Pitch>> GetPitchesByPitchTypeIDAsync(int typeId);
    }
}
