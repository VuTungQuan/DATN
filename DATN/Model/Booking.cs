using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Model
{
    public class Booking
    {
        [Key]
        [Required]
        public int BookingID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int PitchID { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // 'Pending', 'Confirmed', 'Cancelled'

        public DateTime CreatedAt { get; set; } = DateTime.Now;


        // Mối quan hệ với User
        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        // Mối quan hệ với Pitch
        [ForeignKey("PitchID")]
        public virtual Pitch? Pitch { get; set; }

        // Mối quan hệ với Payment (One-to-One)
        public virtual Payment? Payment { get; set; }

    }
}
