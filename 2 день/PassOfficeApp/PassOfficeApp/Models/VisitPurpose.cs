// Models/VisitPurpose.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PassOfficeApp.Models
{
    [Table("visit_purposes", Schema = "kp")]
    public class VisitPurpose
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required]
        [MaxLength(255)]
        public string purpose_name { get; set; }
    }
}