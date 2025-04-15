using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Model
{
    public class PitchType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int PitchTypeID { get; set; }

        public string Name { get; set; }
        public string? ImageUrl { get; set; }
    }
}
