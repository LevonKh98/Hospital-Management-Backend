using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Patient>(e =>
        {
            e.Property(p => p.FirstName).HasMaxLength(80).IsRequired();
            e.Property(p => p.LastName).HasMaxLength(80).IsRequired();
            e.Property(p => p.Phone).HasMaxLength(32);
        });

        // Index to help prevent/spot overlaps later
        b.Entity<Appointment>().HasIndex(a => new { a.DoctorId, a.StartsAt });
    }
}
