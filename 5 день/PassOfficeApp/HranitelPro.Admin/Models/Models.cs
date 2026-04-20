using System;
using System.Collections.Generic;

namespace HranitelPro.Admin.Models
{
    public class User
    {
        public int id { get; set; }
        public string email { get; set; }
        public string login { get; set; }
        public string password_hash { get; set; }
        public DateTime created_at { get; set; }
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

    public class VisitPurpose
    {
        public int id { get; set; }
        public string purpose_name { get; set; }
    }

    public class RequestStatus
    {
        public int id { get; set; }
        public string status_name { get; set; }
    }

    public class GroupMember
    {
        public int id { get; set; }
        public int group_request_id { get; set; }
        public int row_number { get; set; }
        public string full_name_initials { get; set; }
        public string contact_info { get; set; }
        public string photo_path { get; set; }
        public string passport_series { get; set; }
        public string passport_number { get; set; }
        public string last_name { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public DateTime birth_date { get; set; }

        public string DisplayName => $"{row_number}. {full_name_initials}";
    }

    public class RequestViewModel
    {
        public int request_id { get; set; }
        public string request_type { get; set; }
        public int user_id { get; set; }
        public string user_email { get; set; }
        public string user_login { get; set; }
        public int department_id { get; set; }
        public string department_name { get; set; }
        public int employee_id { get; set; }
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
        public int status_id { get; set; }
        public string status { get; set; }
        public string rejection_reason { get; set; }
        public DateTime created_at { get; set; }
        public string full_name { get; set; }
        public string group_leader_name { get; set; }
        public int? members_count { get; set; }
        public string purpose { get; set; }
        public DateTime? visit_start_time { get; set; }
        public DateTime? visit_end_time { get; set; }
        public DateTime? visit_arrival_time { get; set; }

        // Для отображения в таблицах
        public string PassportFull => $"{passport_series} {passport_number}";
        public string VisitStartTimeStr => visit_start_time?.ToString("HH:mm:ss") ?? "—";
        public string VisitEndTimeStr => visit_end_time?.ToString("HH:mm:ss") ?? "—";
        public string VisitArrivalTimeStr => visit_arrival_time?.ToString("HH:mm:ss") ?? "—";

        // Члены группы (для групповых заявок)
        public List<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
    }

    public class BlacklistItem
    {
        public int id { get; set; }
        public string passport_series { get; set; }
        public string passport_number { get; set; }
        public string last_name { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string reason { get; set; }
        public DateTime added_at { get; set; }
        public string added_by_name { get; set; }
    }

    public class ReportData
    {
        public string Period { get; set; }
        public string DepartmentName { get; set; }
        public int VisitCount { get; set; }
        public DateTime Date { get; set; }
    }
}