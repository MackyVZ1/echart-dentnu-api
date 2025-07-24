using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace echart_dentnu_api.Models
{
    [Table("tb_icd10tm")]
    public class tbicd10tmModel
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("code")]
        [StringLength(10, ErrorMessage = "code cannot exceed 10 characters.")]
        public string? Code { get; set; }

        [Column("codeSet")]
        [StringLength(10, ErrorMessage = "codeSet cannot exceed 10 characters.")]
        public string? CodeSet { get; set; }

        [Column("descp")]
        [StringLength(10, ErrorMessage = "descp cannot exceed 255 characters.")]
        public string? Descp { get; set; }
    }
}