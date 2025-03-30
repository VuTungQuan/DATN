using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Model
{
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }

        [Required]
        public int BookingID { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "Cash"; // 'Cash', 'Bank Transfer'

        [Required]
        [MaxLength(50)]
        public string PaymentStatus { get; set; } = "Pending"; // 'Pending', 'Paid', 'Failed'

        public string? TransactionID { get; set; } // Mã giao dịch nếu thanh toán online

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PaidAmount { get; set; }

        public DateTime? PaidDate { get; set; }

        [ForeignKey("BookingID")]
        public virtual Booking? Booking { get; set; }
    }
}
