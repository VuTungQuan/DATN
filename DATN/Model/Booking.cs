using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Model
{
    public class Booking
    {
        [Key]
        public int BookingID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int PitchID { get; set; }
        [Required]
        public int? DiscountID { get; set; }  // Thay vì int DiscountID

        [Required]
        public DateTime BookingDate { get; set; } // Ngày đặt sân

        [Required]
        public TimeSpan StartTime { get; set; } // Giờ bắt đầu

        [Required]
        public TimeSpan EndTime { get; set; } // Giờ kết thúc

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // 'Pending', 'Confirmed', 'Cancelled'

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        [ForeignKey("PitchID")]
        public virtual Pitch? Pitch { get; set; }
        
        [ForeignKey("DiscountID")]
        public virtual Discount? Discount { get; set; }

    }
}
