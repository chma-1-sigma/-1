// Models/PersonalRequest.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PassOfficeApp.Models
{
    [Table("personal_requests", Schema = "kp")]
    public class PersonalRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public int user_id { get; set; }
        public int status_id { get; set; }
        public int purpose_id { get; set; }
        public int department_id { get; set; }
        public int employee_id { get; set; }

        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public string visit_comment { get; set; }

        public string last_name { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string phone { get; set; }
        public string email_visitor { get; set; }
        public string organization { get; set; }
        public DateTime birth_date { get; set; }
        public string passport_series { get; set; }
        public string passport_number { get; set; }
        public string photo_path { get; set; }
        public string passport_scan_path { get; set; }
        public string rejection_reason { get; set; }
        public DateTime created_at { get; set; } = DateTime.Now;

        [ForeignKey("user_id")]
        public virtual User User { get; set; }

        [ForeignKey("status_id")]
        public virtual RequestStatus Status { get; set; }

        [ForeignKey("purpose_id")]
        public virtual VisitPurpose Purpose { get; set; }

        [ForeignKey("department_id")]
        public virtual Department Department { get; set; }

        [ForeignKey("employee_id")]
        public virtual Employee Employee { get; set; }
    }
}