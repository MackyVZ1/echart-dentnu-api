using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace echart_dentnu_api.Models
{
    public class tbdentalrecorduserModel
    {
        [Key]
        [Column("userId")]
        public int UserId { get; set; }

        [Column("license")]
        [StringLength(10, ErrorMessage = "license cannot exceed 10 characters")]
        public string? License { get; set; }

        [Column("fName")]
        [Required(ErrorMessage = "fName cannot be null")]
        [StringLength(50, ErrorMessage = "fName cannot exceed 50 characters")]
        public string Fname { get; set; } = string.Empty;

        [Column("lName")]
        [StringLength(50, ErrorMessage = "lName cannot exceed 50 characters")]
        public string? Lname { get; set; }

        [Column("studentID")]
        [StringLength(15, ErrorMessage = "studentID cannot exceed 15 characters")]
        public string? StudentID { get; set; }

        [Column("roleID")]
        [Required(ErrorMessage = "roleID cannot be null")]
        public int RoleID { get; set; }


        [Column("status")]
        [Required(ErrorMessage = "status cannot be null")]
        public int Status { get; set; }

        [Column("users")]
        [Required(ErrorMessage = "users cannot be null")]
        [StringLength(50, ErrorMessage = "users cannot exceed 50 characters")]
        public string Users { get; set; } = string.Empty;

        [Column("passw")]
        [Required(ErrorMessage = "passw cannot be null")]
        [StringLength(32, ErrorMessage = "passw cannot exceed 32 characters")]
        public string Passw { get; set; } = string.Empty;

        [Column("tName")]
        [StringLength(45, ErrorMessage = "tName cannot exceed 45 characters")]
        public string? Tname { get; set; }

        [Column("sort")]
        [Range(0, 3)]
        public decimal? Sort { get; set; }

        [Column("type")]
        [StringLength(10, ErrorMessage = "type cannot exceed 10 characters")]
        public string? Type { get; set; }

        [Column("clinicid")]
        [StringLength(255, ErrorMessage = "clinicid cannot exceed 255 characters")]
        public string? Clinicid { get; set; }
    }

    public class tbdentalrecorduserDto // แสดงรายชื่อพนักงาน
    {
        [Key]
        [Column("userId")]
        public int UserId { get; set; }

        [Column("license")]
        public string? License { get; set; }

        [Column("fName")]
        public string Fname { get; set; } = string.Empty;

        [Column("lName")]
        public string? Lname { get; set; }

        [Column("studentID")]
        public string? StudentID { get; set; }

        [Column("roleID")]
        public int RoleID { get; set; }

        [Column("tName")]
        public string? Tname { get; set; }

        [Column("clinicid")]
        public string? Clinicid { get; set; }
    }

    public class tbdentalrecorduserPatchDto // สำหรับอัปเดตข้อมูล
    {
        [Column("license")]
        [StringLength(10, ErrorMessage = "license cannot exceed 10 characters")]
        public string? License { get; set; }

        [Column("fName")]
        [StringLength(50, ErrorMessage = "fName cannot exceed 50 characters")]
        public string? Fname { get; set; }

        [Column("lName")]
        [StringLength(50, ErrorMessage = "lName cannot exceed 50 characters")]
        public string? Lname { get; set; }

        [Column("studentID")]
        [StringLength(15, ErrorMessage = "studentID cannot exceed 15 characters")]
        public string? StudentID { get; set; }

        [Column("roleID")]
        [Range(1, 12, ErrorMessage = "RoleID must be between 1 and 12")]
        public int? RoleID { get; set; }

        [Column("status")]
        [Range(0, 1, ErrorMessage = "Status must be 0 or 1")]
        public int? Status { get; set; }

        [Column("users")]
        [StringLength(50, ErrorMessage = "users cannot exceed 50 characters")]
        public string? Users { get; set; }

        [Column("passw")]
        [StringLength(32, ErrorMessage = "passw cannot exceed 32 characters")]
        public string? Passw { get; set; }

        [Column("tName")]
        [StringLength(45, ErrorMessage = "tName cannot exceed 45 characters")]
        public string? Tname { get; set; }

        [Column("sort")]
        [Range(0, 3, ErrorMessage = "Sort must be between 0 and 3")]
        public decimal? Sort { get; set; }

        [Column("type")]
        [StringLength(10, ErrorMessage = "type cannot exceed 10 characters")]
        public string? Type { get; set; }

        [Column("clinicid")]
        [StringLength(255, ErrorMessage = "clinicid cannot exceed 255 characters")]
        public string? Clinicid { get; set; }
    }

    public class tbdentalrecorduserTeacherDto
    {
        [Key]
        [Column("userId")]
        public int UserId { get; set; }

        [Column("tName")]
        public string? Tname { get; set; }

        [Column("fName")]
        public string Fname { get; set; } = string.Empty;

        [Column("lName")]
        public string? Lname { get; set; }
    }

    public class tbdentalrecorduserStudentDto
    {
        [Key]
        [Column("userId")]
        public int UserId { get; set; }

        [Column("tName")]
        public string? Tname { get; set; }

        [Column("fName")]
        public string Fname { get; set; } = string.Empty;

        [Column("lName")]
        public string? Lname { get; set; }
        
        [Column("roleID")]
        public int RoleID { get; set; }
    }
}