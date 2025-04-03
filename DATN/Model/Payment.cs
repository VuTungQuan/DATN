using DATN.Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Payment
{
    [Key]
    public int PaymentID { get; set; }

    [Required]
    public int BookingID { get; set; }

    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = "QR Transfer"; // Thanh toán qua QR

    [Required]
    [MaxLength(50)]
    public string PaymentStatus { get; set; } = "Pending"; // 'Pending', 'Paid', 'Failed'

    public string? TransactionID { get; set; } // Mã giao dịch nếu thanh toán thành công

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PaidAmount { get; set; }

    public DateTime? PaidDate { get; set; }

    [ForeignKey("BookingID")]
    public virtual Booking? Booking { get; set; }
}
