// Models/User.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PassOfficeApp.Models
{
    [Table("users", Schema = "kp")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string email { get; set; }

        [Required]
        [MaxLength(255)]
        public string password_hash { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;
    }
}