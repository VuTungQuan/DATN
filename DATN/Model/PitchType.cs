using System.ComponentModel.DataAnnotations;

namespace DATN.Model
{
    public class PitchType
    {
        [Key]
        public int PitchTypeID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty; // Sân 5, Sân 7, Sân 11

    }
  
}
