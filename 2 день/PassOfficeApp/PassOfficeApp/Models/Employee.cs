// Models/Employee.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PassOfficeApp.Models
{
    [Table("employees", Schema = "kp")]
    public class Employee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public int department_id { get; set; }

        [Required]
        [MaxLength(255)]
        public string full_name { get; set; }

        [MaxLength(255)]
        public string position { get; set; }

        [ForeignKey("department_id")]
        public virtual Department Department { get; set; }
    }
}