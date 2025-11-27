using System.Security.Claims;
using BackendApi.Data;
using BackendApi.Models;
using BackendApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BackendApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]   // -> /api/appointments
    [Authorize(Roles = "Doctor,Nurse,Receptionist,Admin")]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AppointmentsController(AppDbContext db)
        {
            _db = db;
        }

        private int GetCurrentUserId()
        {
            // Adjust claim type name if your JWT uses something else than "userId"
            var claim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
            {
                throw new InvalidOperationException("User id claim is missing from token.");
            }

            return int.Parse(claim.Value);
        }

        // =========================================================
        // POST /api/appointments  -> CREATE
        // =========================================================
        /// <summary>
        /// Create a new appointment with business-hour + overlap validation.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Appointment a)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Always use the currently logged-in user as the staff user
            var currentUserId = GetCurrentUserId();
            a.StaffUserId = currentUserId;

            // 1) business hours / weekday
            var (ok, error) = AppointmentRules.ValidateBusinessWindow(a.StartsAt, a.DurationMinutes, isUtc: false);
            if (!ok)
            {
                return BadRequest(error);
            }

            // 2) overlap check for same staff user (doctor/nurse/receptionist/admin)
            var newStart = a.StartsAt;
            var newEnd = a.StartsAt.AddMinutes(a.DurationMinutes);

            var overlapExists = await _db.Appointments
                .Where(x => x.StaffUserId == a.StaffUserId)
                .AnyAsync(x =>
                    x.StartsAt < newEnd &&
                    x.StartsAt.AddMinutes(x.DurationMinutes) > newStart);

            if (overlapExists)
            {
                return BadRequest("Time conflict for this staff member.");
            }

            // 3) default status
            if (string.IsNullOrWhiteSpace(a.Status))
            {
                a.Status = "Scheduled";
            }

            _db.Appointments.Add(a);
            await _db.SaveChangesAsync();

            // Point to the detail endpoint so frontend can follow this URL.
            return CreatedAtAction(nameof(GetById), new { id = a.Id }, a);
        }

        // =========================================================
        // GET /api/appointments  -> LIST / SEARCH
        // Optional filters: patientId, staffUserId, date=YYYY-MM-DD
        // =========================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetList(
            [FromQuery] int? patientId,
            [FromQuery] int? staffUserId,
            [FromQuery] DateTime? date)
        {
            var query = _db.Appointments.AsQueryable();

            if (patientId.HasValue)
            {
                query = query.Where(a => a.PatientId == patientId.Value);
            }

            if (staffUserId.HasValue)
            {
                query = query.Where(a => a.StaffUserId == staffUserId.Value);
            }

            if (date.HasValue)
            {
                var d = date.Value.Date;
                query = query.Where(a => a.StartsAt.Date == d);
            }

            var results = await query
                .OrderBy(a => a.StartsAt)
                .ToListAsync();

            return Ok(results);
        }

        // =========================================================
        // GET /api/appointments/{id}  -> DETAIL
        // =========================================================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Appointment>> GetById(int id)
        {
            var appointment = await _db.Appointments.FindAsync(id);

            if (appointment == null)
            {
                return NotFound();
            }

            return Ok(appointment);
        }

        // =========================================================
        // PUT /api/appointments/{id}  -> UPDATE
        // =========================================================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Appointment updated)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _db.Appointments.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            // The staff user who is editing becomes the owner for conflict checks
            var currentUserId = GetCurrentUserId();
            existing.StaffUserId = currentUserId;

            // Apply fields that can be changed
            existing.PatientId = updated.PatientId;
            existing.StartsAt = updated.StartsAt;
            existing.DurationMinutes = updated.DurationMinutes;
            existing.Reason = updated.Reason;
            existing.Status = string.IsNullOrWhiteSpace(updated.Status)
                                        ? existing.Status
                                        : updated.Status;

            // Re-run business rules
            var (ok, error) = AppointmentRules.ValidateBusinessWindow(
                existing.StartsAt,
                existing.DurationMinutes,
                isUtc: false);

            if (!ok)
            {
                return BadRequest(error);
            }

            var newStart = existing.StartsAt;
            var newEnd = existing.StartsAt.AddMinutes(existing.DurationMinutes);

            var overlapExists = await _db.Appointments
                .Where(x => x.StaffUserId == existing.StaffUserId && x.Id != existing.Id)
                .AnyAsync(x =>
                    x.StartsAt < newEnd &&
                    x.StartsAt.AddMinutes(x.DurationMinutes) > newStart);

            if (overlapExists)
            {
                return BadRequest("Time conflict for this staff member.");
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // =========================================================
        // DELETE /api/appointments/{id}  -> DELETE / CANCEL
        // =========================================================
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _db.Appointments.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            // Hard delete:
            _db.Appointments.Remove(existing);

            // If you want soft cancel instead, you could do:
            // existing.Status = "Cancelled";
            // await _db.SaveChangesAsync();
            // return NoContent();

            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
