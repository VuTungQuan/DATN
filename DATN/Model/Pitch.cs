using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Model
{
    public class Pitch
    {
        [Key]
        public int PitchID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        public string Status { get; set; }

        public bool IsCombined { get; set; }

        public int? ParentPitchID { get; set; }

        public string? ImageUrl { get; set; }

        [Required]
        public int PitchTypeID { get; set; } 

        [ForeignKey("PitchTypeID")]
        public virtual PitchType? PitchType { get; set; }
    }
}
