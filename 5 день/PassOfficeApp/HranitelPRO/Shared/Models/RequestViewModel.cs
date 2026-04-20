using System;

namespace HranitelPRO_Shared.Models
{
    public class RequestViewModel
    {
        public int request_id { get; set; }
        public string request_type { get; set; }
        public int user_id { get; set; }
        public string user_email { get; set; }
        public string department_name { get; set; }
        public string employee_name { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public string visit_comment { get; set; }
        public string last_name { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string phone { get; set; }
        public string visitor_email { get; set; }
        public string organization { get; set; }
        public DateTime? birth_date { get; set; }
        public string passport_series { get; set; }
        public string passport_number { get; set; }
        public string photo_path { get; set; }
        public string passport_scan_path { get; set; }
        public string status { get; set; }
        public string rejection_reason { get; set; }
        public DateTime created_at { get; set; }
        public string full_name { get; set; }
        public string group_leader_name { get; set; }
        public int? members_count { get; set; }
        public string purpose { get; set; }
        public DateTime? visit_start_time { get; set; }
        public DateTime? visit_end_time { get; set; }

        // Дополнительные свойства
        public string PassportFull { get; set; }

        public int Id { get { return request_id; } set { request_id = value; } }
        public string RequestType { get { return request_type; } set { request_type = value; } }
        public string DepartmentName { get { return department_name; } set { department_name = value; } }
        public DateTime StartDate { get { return start_date; } set { start_date = value; } }
        public DateTime EndDate { get { return end_date; } set { end_date = value; } }
        public string Status { get { return status; } set { status = value; } }
        public string RejectionReason { get { return rejection_reason; } set { rejection_reason = value; } }
        public DateTime CreatedAt { get { return created_at; } set { created_at = value; } }
        public string FullName { get { return full_name; } set { full_name = value; } }
        public string Phone { get { return phone; } set { phone = value; } }
        public string VisitorEmail { get { return visitor_email; } set { visitor_email = value; } }
        public string PassportSeries { get { return passport_series; } set { passport_series = value; } }
        public string PassportNumber { get { return passport_number; } set { passport_number = value; } }
        public string UserEmail { get { return user_email; } set { user_email = value; } }
        public string GroupLeaderName { get { return group_leader_name; } set { group_leader_name = value; } }
    }

    public class User
    {
        public int id { get; set; }
        public string email { get; set; }
        public string login { get; set; }
        public string password_hash { get; set; }
        public DateTime created_at { get; set; } = DateTime.Now;
    }

    public class Department
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Employee
    {
        public int id { get; set; }
        public int department_id { get; set; }
        public string full_name { get; set; }
        public string position { get; set; }
    }
}