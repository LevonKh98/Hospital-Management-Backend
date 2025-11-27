using System;

namespace BackendApi.Models
{
    public class Appointment
    {
        public int Id { get; set; }                 // PK

        // Patient this appointment is for
        public int PatientId { get; set; }          // FK -> Patients.Id
        public Patient? Patient { get; set; }

        // Staff user (Doctor/Nurse/Receptionist) who created/handles this appointment
        public int StaffUserId { get; set; }        // FK -> Users.UserId
        public User? StaffUser { get; set; }

        public DateTime StartsAt { get; set; }      // store UTC
        public int DurationMinutes { get; set; }    // flexible per appt
        public string? Reason { get; set; }

        // Allowed values (validated in code): "Scheduled" | "Completed" | "Cancelled" | "NoShow"
        public string Status { get; set; } = "Scheduled";
    }
}
