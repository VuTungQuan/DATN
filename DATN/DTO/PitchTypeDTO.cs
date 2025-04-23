using System.ComponentModel.DataAnnotations;

namespace DATN.DTO
{
    // DTO cho thông tin loại sân trả về từ API
    public class PitchTypeDTO
    {
        public int PitchTypeID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }

    // DTO cho việc tạo loại sân mới
    public class PitchTypeCreateDTO
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }
    }

    // DTO cho việc cập nhật thông tin loại sân
    public class PitchTypeUpdateDTO
    {
        [MaxLength(50)]
        public string? Name { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }
    }
} 