using BackendApi.Data;
using BackendApi.Models;
using BackendApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]   // -> /api/appointments
    public class AppointmentsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AppointmentsController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Create a new appointment with business-hour + overlap validation.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Appointment a)
        {
            // 1) business hours / weekday
            var (ok, error) = AppointmentRules.ValidateBusinessWindow(a.StartsAt, a.DurationMinutes, isUtc: false);
            if (!ok)
            {
                return BadRequest(error);
            }

            // 2) overlap check for same doctor
            var newStart = a.StartsAt;
            var newEnd = a.StartsAt.AddMinutes(a.DurationMinutes);

            var overlapExists = await _db.Appointments
                .Where(x => x.DoctorId == a.DoctorId)
                .AnyAsync(x =>
                    x.StartsAt < newEnd &&
                    x.StartsAt.AddMinutes(x.DurationMinutes) > newStart);

            if (overlapExists)
            {
                return BadRequest("Time conflict for this doctor.");
            }

            // 3) default status
            if (string.IsNullOrWhiteSpace(a.Status))
            {
                a.Status = "Scheduled";
            }

            _db.Appointments.Add(a);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Create), new { id = a.Id }, a);
        }
    }
}
