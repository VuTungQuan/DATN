using System;
using System.ComponentModel.DataAnnotations;

namespace DATN.DTO
{
    // DTO cho thông tin đặt sân trả về từ API
    public class BookingDTO
    {
        public int BookingID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int PitchID { get; set; }
        public string PitchName { get; set; } = string.Empty;
        public string PitchTypeName { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public PaymentDTO? Payment { get; set; }
    }

    // DTO cho việc tạo đặt sân mới
    public class BookingCreateDTO
    {
        [Required]
        public int PitchID { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalPrice { get; set; }
    }

    // DTO cho việc cập nhật thông tin đặt sân
    public class BookingUpdateDTO
    {
        public int? PitchID { get; set; }
        public DateTime? BookingDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public decimal? TotalPrice { get; set; }
        public string? Status { get; set; }
    }

    // DTO cho tìm kiếm đặt sân
    public class BookingSearchDTO
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? PitchID { get; set; }
        public int? UserID { get; set; }
        public string? Status { get; set; }
    }

    // DTO cho việc đặt sân - bao gồm thông tin thanh toán
    public class BookingRequestDTO
    {
        [Required]
        public BookingCreateDTO Booking { get; set; } = new BookingCreateDTO();
        
        public PaymentCreateDTO? Payment { get; set; }
    }
} 