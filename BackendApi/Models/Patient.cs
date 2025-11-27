using System;
using System.Collections.Generic;

namespace BackendApi.Models
{
    public class Patient
    {
        public int Id { get; set; }                 // PK
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string? Phone { get; set; }

        public string? Email { get; set; }
        public string? Address { get; set; }

        // Relationships
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<PatientNote> Notes { get; set; } = new List<PatientNote>();
    }
}
