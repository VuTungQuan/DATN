using DATN.Model;

namespace DATN.Repositories
{
    public interface IPitchTypeRepository
    {
        Task<IEnumerable<PitchType>> GetAllPitchTypesAsync();  // Lấy tất cả các loại sân
        Task<PitchType> GetPitchTypeByIdAsync(int id);
        Task<PitchType> GetPitchTypeByNameAsync(string name);// Lấy thông tin loại sân theo ID
        Task AddPitchTypeAsync(PitchType pitchType);  // Thêm mới loại sân
        Task UpdatePitchTypeAsync(PitchType pitchType);  // Cập nhật loại sân
        Task DeletePitchTypeAsync(int id);  // Xóa loại sân
    }
}