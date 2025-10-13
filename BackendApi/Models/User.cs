namespace BackendApi.Models
{
    public class User
    {
        public int UserId { get; set; }           // PK
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;     // UNIQUE
        public string Username { get; set; } = null!;  // UNIQUE (you said keep username)
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = null!;      // "Admin" | "Doctor" | "Nurse" | "Receptionist"
        public bool IsActive { get; set; } = true;

        // Optional: navigation collections (not required for migration to work)
        public ICollection<Appointment>? DoctorAppointments { get; set; }  // when Role=Doctor
        public ICollection<PatientNote>? NotesAuthored { get; set; }       // as doctor
    }
}
