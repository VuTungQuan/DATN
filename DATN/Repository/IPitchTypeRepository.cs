using DATN.Model;

namespace DATN.Repository
{
    public interface IPitchTypeRepository
    {
        Task<IEnumerable<PitchType>> GetAllPitchTypesAsync();  // Lấy tất cả các loại sân
        Task<PitchType?> GetPitchTypeByIdAsync(int id);
        Task<IEnumerable<PitchType>> GetPitchTypeByNameAsync(string name);
        Task AddPitchTypeAsync(PitchType pitchType);  // Thêm mới loại sân
        Task UpdatePitchTypeAsync(PitchType pitchType);  // Cập nhật loại sân
        Task DeletePitchTypeAsync(int id);  // Xóa loại sân
    }
}