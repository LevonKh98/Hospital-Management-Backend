using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Appointment> Appointments => Set<Appointment>();

        // NEW
        public DbSet<User> Users => Set<User>();
        public DbSet<PatientNote> PatientNotes => Set<PatientNote>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // ===== Users =====
            b.Entity<User>(e =>
            {
                e.HasKey(x => x.UserId);
                e.Property(x => x.FullName).IsRequired();
                e.Property(x => x.Email).IsRequired();
                e.Property(x => x.Username).IsRequired();
                e.Property(x => x.PasswordHash).IsRequired();
                e.Property(x => x.Role).IsRequired();

                // Unique constraints
                e.HasIndex(x => x.Email).IsUnique();
                e.HasIndex(x => x.Username).IsUnique();
            });

            // ===== Patients =====
            b.Entity<Patient>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.FirstName).IsRequired();
                e.Property(x => x.LastName).IsRequired();
                e.Property(x => x.DateOfBirth).IsRequired();
                // Email and Address are nullable by design

                // Helpful name search (optional)
                e.HasIndex(x => x.LastName);
            });

            // ===== Appointments =====
            b.Entity<Appointment>(e =>
            {
                e.HasKey(x => x.Id);

                // FK: Appointment.PatientId -> Patients.Id (Restrict)
                e.HasOne<Patient>()
                 .WithMany()
                 .HasForeignKey(x => x.PatientId)
                 .OnDelete(DeleteBehavior.Restrict);

                // FK: Appointment.DoctorId -> Users.UserId (Restrict)
                e.HasOne<User>()
                 .WithMany()
                 .HasForeignKey(x => x.DoctorId)
                 .OnDelete(DeleteBehavior.Restrict);

                // Indexes for performance
                e.HasIndex(x => new { x.DoctorId, x.StartsAt });
                e.HasIndex(x => new { x.PatientId, x.StartsAt });

                // Status required with default "Scheduled"
                e.Property(x => x.Status)
                 .IsRequired()
                 .HasDefaultValue("Scheduled");
            });

            // ===== PatientNotes =====
            b.Entity<PatientNote>(e =>
            {
                e.HasKey(x => x.NoteId);

                // FK: Note.PatientId -> Patients.Id (Restrict)
                e.HasOne<Patient>(x => x.Patient!)
                 .WithMany()
                 .HasForeignKey(x => x.PatientId)
                 .OnDelete(DeleteBehavior.Restrict);

                // FK: Note.DoctorId -> Users.UserId (Restrict)
                e.HasOne<User>(x => x.Doctor!)
                 .WithMany()
                 .HasForeignKey(x => x.DoctorId)
                 .OnDelete(DeleteBehavior.Restrict);

                // FK: Note.AppointmentId -> Appointments.Id (SetNull)
                e.HasOne<Appointment>(x => x.Appointment!)
                 .WithMany()
                 .HasForeignKey(x => x.AppointmentId)
                 .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
