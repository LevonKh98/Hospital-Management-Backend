namespace BackendApi.Models
{
    public class Patient
    {
        public int Id { get; set; }                 // PK
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string? Phone { get; set; }

        // NEW (you asked to add these)
        public string? Email { get; set; }
        public string? Address { get; set; }
    }
}
