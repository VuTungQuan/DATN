using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Model
{
    public class Pitch
    {
        [Key]
        [Required]
        public int PitchID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
  
        [Required]
        public int PitchTypeID { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
        public string? Status { get; set; }
        public bool IsCombined { get; set; }

        public string? ImageUrl { get; set; }

        // Thêm trường Description để lưu thông tin sân gộp
        public string? Description { get; set; }

        // Mối quan hệ với PitchType
        [ForeignKey("PitchTypeID")]
        public virtual PitchType PitchType { get; set; }

        public virtual ICollection<Booking>? Bookings { get; set; }
    }

}
