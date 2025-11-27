using System;
using System.Collections.Generic;

namespace BackendApi.Models
{
    public class PatientSummaryDto
    {
        public int PatientId { get; set; }
        public string FullName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }

        public List<AppointmentSummaryDto> Appointments { get; set; } = new();
        public List<PatientNoteSummaryDto> Notes { get; set; } = new();
    }

    public class AppointmentSummaryDto
    {
        public int AppointmentId { get; set; }
        public DateTime StartsAt { get; set; }
        public int DurationMinutes { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = null!;
        public int StaffUserId { get; set; }
    }

    public class PatientNoteSummaryDto
    {
        public int NoteId { get; set; }
        public string Text { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public int AuthorUserId { get; set; }
    }
}
