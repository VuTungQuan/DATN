using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DATN.Model
{
    public class Discount
    {
        [Key]
        public int DiscountID { get; set; }

        [Required]
        [MaxLength(50)]
        public string DiscountCode { get; set; }  // Mã giảm giá

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; }  // Số tiền giảm

        [Required]
        public DateTime StartDate { get; set; }  // Ngày bắt đầu

        [Required]
        public DateTime EndDate { get; set; }  // Ngày kết thúc

        [Required]
        public bool IsActive { get; set; }  // Trạng thái hoạt động của mã giảm giá
    }

}
