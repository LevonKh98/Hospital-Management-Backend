namespace BackendApi.Models
{
    public class PatientNote
    {
        public int NoteId { get; set; }            // PK

        public int PatientId { get; set; }         // FK -> Patients.Id
        public int DoctorId { get; set; }          // FK -> Users.UserId (author of note)
        public int? AppointmentId { get; set; }    // optional FK -> Appointments.Id

        public string Text { get; set; } = null!;

        // Optional navigations (helpful but not required)
        public Patient? Patient { get; set; }
        public User? Doctor { get; set; }
        public Appointment? Appointment { get; set; }
    }
}
