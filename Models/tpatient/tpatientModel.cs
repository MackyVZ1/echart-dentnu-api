using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace echart_dentnu_api.Models
{
    public class tpatientModel
    {
        [Key]
        [Column("dn")]
        [Required(ErrorMessage = "dn cannot be null")]
        [StringLength(10, ErrorMessage = "dn cannot exceed 10 characters.")]
        public string DN { get; set; } = string.Empty;

        [Column("titleTh")]
        [StringLength(100, ErrorMessage = "titleTh cannot exceed 100 characters.")]
        public string? TitleTh { get; set; }

        [Column("nameTh")]
        [StringLength(100, ErrorMessage = "nameTh cannot be exceed 100 characters.")]
        public string? NameTh { get; set; }

        [Column("surnameTh")]
        [StringLength(100, ErrorMessage = "surnameTh cannot be exceed 100 characters.")]
        public string? SurnameTh { get; set; }

        [Column("titleEn")]
        [Required(ErrorMessage = "titleEn cannot be null")]
        [StringLength(45, ErrorMessage = "titleEn cannot be exceed 45 characters.")]
        public string TitleEn { get; set; } = string.Empty;

        [Column("nameEn")]
        [Required(ErrorMessage = "nameEn cannot be null")]
        [StringLength(45, ErrorMessage = "nameEn cannot be exceed 45 characters.")]
        public string NameEn { get; set; } = string.Empty;

        [Column("surnameEn")]
        [Required(ErrorMessage = "surnameEn cannot be null")]
        [StringLength(45, ErrorMessage = "surnameEn cannot be exceed 45 characters.")]
        public string SurnameEn { get; set; } = string.Empty;

        [Column("sex")]
        [Required(ErrorMessage = "sex cannot be null")]
        [StringLength(45, ErrorMessage = "sex cannot be exceed 45 characters.")]
        public string Sex { get; set; } = string.Empty;

        [Column("maritalStatus")]
        [Required(ErrorMessage = "maritalStatus cannot be null")]
        [StringLength(45, ErrorMessage = "maritalStatus cannot be exceed 45 characters.")]
        public string MaritalStatus { get; set; } = string.Empty;

        [Column("idNo")]
        [StringLength(50, ErrorMessage = "idNo cannot be exceed 50 characters.")]
        public string? IdNo { get; set; }

        [Column("age")]
        [Required(ErrorMessage = "age cannot be null")]
        [StringLength(45, ErrorMessage = "age cannot be exceed 45 characters.")]
        public string Age { get; set; } = string.Empty;

        [Column("occupation")]
        [Required(ErrorMessage = "occupation cannot be null")]
        [StringLength(45, ErrorMessage = "occupation cannot be exceed 45 characters.")]
        public string Occupation { get; set; } = string.Empty;

        [Column("address")]
        [StringLength(255, ErrorMessage = "address cannot be exceed 255 characters.")]
        public string? Address { get; set; }

        [Column("phoneHome")]
        [StringLength(255, ErrorMessage = "phoneHome cannot be exceed 255 characters.")]
        public string? PhoneHome { get; set; }

        [Column("phoneOffice")]
        [Required(ErrorMessage = "phoneOffice cannot be null")]
        [StringLength(45, ErrorMessage = "phoneOffice cannot be exceed 45 characters.")]
        public string PhoneOffice { get; set; } = string.Empty;

        [Column("emerNotify")]
        [Required(ErrorMessage = "emerNotify cannot be null")]
        [StringLength(45, ErrorMessage = "emerNotify cannot be exceed 45 characters.")]
        public string EmerNotify { get; set; } = string.Empty;

        [Column("emerAddress")]
        [Required(ErrorMessage = "emerAddress cannot be null")]
        [StringLength(255, ErrorMessage = "emerAddress cannot be exceed 45 characters.")]
        public string EmerAddress { get; set; } = string.Empty;

        [Column("parent")]
        [Required(ErrorMessage = "parent cannot be null")]
        [StringLength(45, ErrorMessage = "parent cannot be exceed 45 characters.")]
        public string Parent { get; set; } = string.Empty;

        [Column("parentPhone")]
        [Required(ErrorMessage = "parentPhone cannot be null")]
        [StringLength(45, ErrorMessage = "parentPhone cannot be exceed 45 characters.")]
        public string ParentPhone { get; set; } = string.Empty;

        [Column("physician")]
        [Required(ErrorMessage = "physician cannot be null")]
        [StringLength(45, ErrorMessage = "physician cannot be exceed 45 characters.")]
        public string Physician { get; set; } = string.Empty;

        [Column("physicianOffice")]
        [Required(ErrorMessage = "physicianOffice cannot be null")]
        [StringLength(45, ErrorMessage = "physicianOffice cannot be exceed 45 characters.")]
        public string PhysicianOffice { get; set; } = string.Empty;

        [Column("physicianPhone")]
        [Required(ErrorMessage = "physicianPhone cannot be null")]
        [StringLength(45, ErrorMessage = "physicianPhone cannot be exceed 45 characters.")]
        public string PhysicianPhone { get; set; } = string.Empty;

        [Column("regDate")]
        [StringLength(50, ErrorMessage = "regDate cannot be exceed 50 characters.")]
        public string? RegDate { get; set; }

        [Column("birthDate")]
        [StringLength(50, ErrorMessage = "birthDate cannot be exceed 50 characters.")]
        public string? BirthDate { get; set; }

        [Column("priv")]
        [StringLength(45, ErrorMessage = "priv cannot be exceed 45 characters.")]
        public string? Priv { get; set; }

        [Column("otherAddress")]
        [Required(ErrorMessage = "otherAddress cannot be null")]
        [StringLength(255, ErrorMessage = "otherAddress cannot be exceed 255 characters.")]
        public string OtherAddress { get; set; } = string.Empty;

        [Column("rdate", TypeName = "DATE")]
        public DateTime? Rdate { get; set; }

        [Column("bdate", TypeName = "DATE")]
        public DateTime? Bdate { get; set; }

        [Column("fromHospital")]
        [StringLength(255, ErrorMessage = "fromHospital cannot be exceed 255 characters.")]
        public string? FromHospital { get; set; }

        [Column("updateByUserId")]
        public int? UpdateByUserId { get; set; }

        [Column("updateTime")]
        [Required]
        public DateTime UpdateTime { get; set; }
    }

    public class tpatientDto
    {
        [Key]
        [Column("dn")]
        public string DN { get; set; } = string.Empty;

        [Column("titleTh")]

        public string? TitleTh { get; set; }

        [Column("nameTh")]
        public string? NameTh { get; set; }

        [Column("surnameTh")]
        public string? SurnameTh { get; set; }

        [Column("idNo")]
        public string? IdNo { get; set; }
    }

    public class tpatientPatchDto
    {
        [Column("titleTh")]
        [StringLength(100, ErrorMessage = "titleTh cannot exceed 100 characters.")]
        public string? TitleTh { get; set; }

        [Column("nameTh")]
        [StringLength(100, ErrorMessage = "nameTh cannot be exceed 100 characters.")]
        public string? NameTh { get; set; }

        [Column("surnameTh")]
        [StringLength(100, ErrorMessage = "surnameTh cannot be exceed 100 characters.")]
        public string? SurnameTh { get; set; }

        [Column("titleEn")]
        [StringLength(45, ErrorMessage = "titleEn cannot be exceed 45 characters.")]
        public string? TitleEn { get; set; }

        [Column("nameEn")]
        [StringLength(45, ErrorMessage = "nameEn cannot be exceed 45 characters.")]
        public string? NameEn { get; set; }

        [Column("surnameEn")]
        [StringLength(45, ErrorMessage = "surnameEn cannot be exceed 45 characters.")]
        public string? SurnameEn { get; set; }

        [Column("sex")]
        [StringLength(45, ErrorMessage = "sex cannot be exceed 45 characters.")]
        public string? Sex { get; set; }

        [Column("maritalStatus")]
        [StringLength(45, ErrorMessage = "maritalStatus cannot be exceed 45 characters.")]
        public string? MaritalStatus { get; set; }

        [Column("idNo")]
        [StringLength(50, ErrorMessage = "idNo cannot be exceed 50 characters.")]
        public string? IdNo { get; set; }

        [Column("age")]
        [StringLength(45, ErrorMessage = "age cannot be exceed 45 characters.")]
        public string? Age { get; set; }

        [Column("occupation")]
        [StringLength(45, ErrorMessage = "occupation cannot be exceed 45 characters.")]
        public string? Occupation { get; set; }

        [Column("address")]
        [StringLength(255, ErrorMessage = "address cannot be exceed 255 characters.")]
        public string? Address { get; set; }

        [Column("phoneHome")]
        [StringLength(255, ErrorMessage = "phoneHome cannot be exceed 255 characters.")]
        public string? PhoneHome { get; set; }

        [Column("phoneOffice")]
        [StringLength(45, ErrorMessage = "phoneOffice cannot be exceed 45 characters.")]
        public string? PhoneOffice { get; set; }

        [Column("emerNotify")]
        [StringLength(45, ErrorMessage = "emerNotify cannot be exceed 45 characters.")]
        public string? EmerNotify { get; set; }

        [Column("emerAddress")]
        [StringLength(255, ErrorMessage = "emerAddress cannot be exceed 45 characters.")]
        public string? EmerAddress { get; set; }

        [Column("parent")]
        [StringLength(45, ErrorMessage = "parent cannot be exceed 45 characters.")]
        public string? Parent { get; set; }

        [Column("parentPhone")]
        [StringLength(45, ErrorMessage = "parentPhone cannot be exceed 45 characters.")]
        public string? ParentPhone { get; set; }

        [Column("physician")]
        [StringLength(45, ErrorMessage = "physician cannot be exceed 45 characters.")]
        public string? Physician { get; set; }

        [Column("physicianOffice")]
        [StringLength(45, ErrorMessage = "physicianOffice cannot be exceed 45 characters.")]
        public string? PhysicianOffice { get; set; }

        [Column("physicianPhone")]
        [StringLength(45, ErrorMessage = "physicianPhone cannot be exceed 45 characters.")]
        public string? PhysicianPhone { get; set; }

        [Column("regDate")]
        [StringLength(50, ErrorMessage = "regDate cannot be exceed 50 characters.")]
        public string? RegDate { get; set; }

        [Column("birthDate")]
        [StringLength(50, ErrorMessage = "birthDate cannot be exceed 50 characters.")]
        public string? BirthDate { get; set; }

        [Column("priv")]
        [StringLength(45, ErrorMessage = "priv cannot be exceed 45 characters.")]
        public string? Priv { get; set; }

        [Column("otherAddress")]
        [StringLength(255, ErrorMessage = "otherAddress cannot be exceed 255 characters.")]
        public string? OtherAddress { get; set; }

        [Column("rdate", TypeName = "DATE")]
        public DateTime? Rdate { get; set; }

        [Column("bdate", TypeName = "DATE")]
        public DateTime? Bdate { get; set; }

        [Column("fromHospital")]
        [StringLength(255, ErrorMessage = "fromHospital cannot be exceed 255 characters.")]
        public string? FromHospital { get; set; }

        [Column("updateByUserId")]
        [Required]
        public int UpdateByUserId { get; set; }

        [Column("updateTime")]
        [Required]
        public DateTime UpdateTime { get; set; }
    }

}