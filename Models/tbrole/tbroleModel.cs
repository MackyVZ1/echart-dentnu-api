using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace echart_dentnu_api.Models
{
    [Table("tbrole")]
    public class tbroleModel
    {
        [Key]
        [Column("roleId")]
        public int RoleID { get; set; }

        [Column("roleName")]
        [Required(ErrorMessage = "roleName cannot be null.")]
        [StringLength(50, ErrorMessage = "roleName cannot exceed 50 characters.")]
        public string RoleName { get; set; } = string.Empty;
    }
}