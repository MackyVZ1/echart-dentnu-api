using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace echart_dentnu_api.Models
{
    [Table("tbclinic")]
    public class tbclinicModel
    {
        [Key]
        [Column("clinicID")]
        public int Clinicid { get; set; }

        [Column("clinicName")]
        [Required(ErrorMessage = "clinicName cannot be null.")]
        [StringLength(250, ErrorMessage = "clinicName cannot exceed 250 characters.")]
        public string ClinicName { get; set; } = string.Empty;
    }
}