using Microsoft.EntityFrameworkCore;
using BackendApi.Models;

namespace BackendApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
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
                e.Property(x => x.IsActive).HasDefaultValue(true);

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
                // Email, Address, Phone are nullable by design

                // Helpful name search (optional)
                e.HasIndex(x => x.LastName);
            });

            // ===== Appointments =====
            b.Entity<Appointment>(e =>
            {
                e.HasKey(x => x.Id);

                // FK: Appointment.PatientId -> Patients.Id (Restrict)
                e.HasOne(x => x.Patient)
                 .WithMany(p => p.Appointments)
                 .HasForeignKey(x => x.PatientId)
                 .OnDelete(DeleteBehavior.Restrict);

                // FK: Appointment.StaffUserId -> Users.UserId (Restrict)
                // (Doctor / Nurse / Receptionist – whoever handles the appt)
                e.HasOne(x => x.StaffUser)
                 .WithMany() // no collection on User (keeps User clean)
                 .HasForeignKey(x => x.StaffUserId)
                 .OnDelete(DeleteBehavior.Restrict);

                // Indexes for performance
                e.HasIndex(x => new { x.StaffUserId, x.StartsAt });
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

                e.Property(x => x.Text).IsRequired();
                e.Property(x => x.CreatedAt).IsRequired();

                // FK: Note.PatientId -> Patients.Id (Restrict)
                e.HasOne(x => x.Patient)
                 .WithMany(p => p.Notes)
                 .HasForeignKey(x => x.PatientId)
                 .OnDelete(DeleteBehavior.Cascade);


                // FK: Note.AuthorUserId -> Users.UserId (Restrict)
                // (Doctor/Nurse/Receptionist who wrote the note)
                e.HasOne(x => x.AuthorUser)
                 .WithMany()
                 .HasForeignKey(x => x.AuthorUserId)
                 .OnDelete(DeleteBehavior.Restrict);

                // FK: Note.AppointmentId -> Appointments.Id (SetNull)
                e.HasOne(x => x.Appointment)
                 .WithMany()
                 .HasForeignKey(x => x.AppointmentId)
                 .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
