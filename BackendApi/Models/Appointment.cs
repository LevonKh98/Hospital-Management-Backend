namespace BackendApi.Models;

public class Appointment
{
    public int Id { get; set; }
    public DateTime StartsAt { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public string Reason { get; set; } = string.Empty;

    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    // Simple for now; we’ll add a Doctor table later
    public int DoctorId { get; set; }
}
