namespace BackendApi.Models
{
    public class Appointment
    {
        public int Id { get; set; }                 // PK
        public int PatientId { get; set; }          // FK -> Patients.Id
        public int DoctorId { get; set; }           // FK -> Users.UserId (Role=Doctor)
        public DateTime StartsAt { get; set; }      // store UTC
        public int DurationMinutes { get; set; }    // flexible per appt
        public string? Reason { get; set; }

        // NEW (you asked to add this)
        // Allowed values you validate in code: "Scheduled" | "Completed" | "Cancelled" | "NoShow"
        public string Status { get; set; } = "Scheduled";
    }
}
