namespace BackendApi.Models
{
    public class PatientNote
    {
        public int NoteId { get; set; }            // PK

        public int PatientId { get; set; }         // FK -> Patients.Id
        public Patient? Patient { get; set; }

        public int AuthorUserId { get; set; }      // FK -> Users.UserId (author of note)
        public User? AuthorUser { get; set; }

        public int? AppointmentId { get; set; }    // optional FK -> Appointments.Id
        public Appointment? Appointment { get; set; }

        public string Text { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // good practice
    }
}
