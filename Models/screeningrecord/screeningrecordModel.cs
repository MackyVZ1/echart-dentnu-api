using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace echart_dentnu_api.Models
{
    [Table("screeningrecord")]
    public class screeningrecordModel
    {
        [Key]
        [Column("screeningId")]
        public int screeningId { get; set; }

        [Column("dn")]
        [StringLength(10, ErrorMessage = "other cannot be exceed 10 characters.")]
        public string? dn { get; set; }

        [Column("sys")]
        public uint? sys { get; set; }

        [Column("dia")]
        public uint? dia { get; set; }

        [Column("pr")]
        public uint? pr { get; set; }

        [Column("temperature")]
        public uint? temperature { get; set; }

        [Column("treatmentUrgency")]
        public treatmentUrgency treatmentUrgency { get; set; }

        [Column("bloodpressure")]
        public bool? bloodpressure { get; set; } = null;

        [Column("diabete")]
        public bool? diabete { get; set; } = null;

        [Column("heartdisease")]
        public bool? heartdisease { get; set; } = null;

        [Column("thyroid")]
        public bool? thyroid { get; set; } = null;

        [Column("stroke")]
        public bool? stroke { get; set; } = null;

        [Column("immunodeficiency")]
        public bool? immunodeficiency { get; set; } = null;

        [Column("pregnant")]
        public uint? pregnant { get; set; }

        [Column("other")]
        [StringLength(100, ErrorMessage = "other cannot be exceed 100 characters.")]
        public string? other { get; set; }

        [Column("createdAt")]
        public DateTime? createdAt { get; set; }

        [Column("updateAt")]
        public DateTime? updateAt { get; set; }
    }

    public enum treatmentUrgency
    {
        emergency,
        urgency,
        nonurgency
    }

}