using System;
using System.ComponentModel.DataAnnotations;

namespace DATN.DTO
{
    // DTO cho thông tin sân trả về từ API
    public class PitchDTO
    {
        public int PitchID { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PitchTypeID { get; set; }
        public string PitchTypeName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsCombined { get; set; }
        public string? ImageUrl { get; set; }
    }

    // DTO cho việc tạo sân mới
    public class PitchCreateDTO
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int PitchTypeID { get; set; }

        [Required]
        [Range(0, 10000000)]
        public decimal Price { get; set; }

        public string Status { get; set; } = "Hoạt động";
        
        public bool IsCombined { get; set; } = false;
    }

    // DTO cho việc cập nhật thông tin sân
    public class PitchUpdateDTO
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        public int? PitchTypeID { get; set; }

        [Range(0, 10000000)]
        public decimal? Price { get; set; }

        public string? Status { get; set; }
        
        public bool? IsCombined { get; set; }
    }

    // DTO cho tìm kiếm sân
    public class PitchSearchDTO
    {
        public int? PitchTypeID { get; set; }
        public string? Status { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? IsCombined { get; set; }
    }

    // DTO cho danh sách sân trống
    public class AvailablePitchDTO
    {
        public int PitchID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PitchTypeName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public List<TimeSlotDTO> AvailableTimeSlots { get; set; } = new List<TimeSlotDTO>();
    }

    // DTO cho khung giờ trống
    public class TimeSlotDTO
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }
} 