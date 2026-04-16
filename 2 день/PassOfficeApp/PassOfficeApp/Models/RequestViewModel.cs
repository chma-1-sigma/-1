// Models/RequestViewModel.cs
using System;

namespace PassOfficeApp.Models
{
    public class RequestViewModel
    {
        public int Id { get; set; }
        public string RequestType { get; set; }
        public string DepartmentName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public string RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}