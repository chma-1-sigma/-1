// Models/RequestStatus.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PassOfficeApp.Models
{
    [Table("request_statuses", Schema = "kp")]
    public class RequestStatus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required]
        [MaxLength(50)]
        public string status_name { get; set; }
    }
}