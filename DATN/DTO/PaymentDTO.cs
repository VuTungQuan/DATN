using System;
using System.ComponentModel.DataAnnotations;

namespace DATN.DTO
{
    // DTO cho thông tin thanh toán trả về từ API
    public class PaymentDTO
    {
        public int PaymentID { get; set; }
        public int BookingID { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? TransactionID { get; set; }
        public DateTime PaymentDate { get; set; }
    }

    // DTO cho việc tạo thanh toán mới
    public class PaymentCreateDTO
    {
        [Required]
        public int BookingID { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "Cash"; // Cash, Credit Card, VNPay, etc.
        
        public string? TransactionID { get; set; }
    }

    // DTO cho việc cập nhật thông tin thanh toán
    public class PaymentUpdateDTO
    {
        public decimal? Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Status { get; set; }
        public string? TransactionID { get; set; }
    }

    // DTO cho thông tin thanh toán online
    public class OnlinePaymentRequestDTO
    {
        [Required]
        public int BookingID { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = string.Empty; // VNPay, MoMo, etc.

        public string? ReturnUrl { get; set; }
    }

    // DTO cho kết quả thanh toán online
    public class OnlinePaymentResponseDTO
    {
        public bool Success { get; set; }
        public string PaymentUrl { get; set; } = string.Empty;
        public string TransactionID { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
} 