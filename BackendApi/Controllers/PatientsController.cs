using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BackendApi.Data;
using BackendApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]   // -> /api/patients
    [Authorize(Roles = "Doctor,Nurse,Receptionist,Admin")]
    public class PatientsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public PatientsController(AppDbContext db)
        {
            _db = db;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
            {
                throw new InvalidOperationException("User id claim is missing from token.");
            }

            return int.Parse(claim.Value);
        }

        // =========================================================
        // GET: api/patients  -> Get All Patients
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var patients = await _db.Patients
                .AsNoTracking()
                .ToListAsync();

            return Ok(patients);
        }

        // =========================================================
        // GET: api/patients/search?q=
        // =========================================================
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Ok(Array.Empty<Patient>());

            var term = q.Trim().ToLower();

            var patients = await _db.Patients
                .AsNoTracking()
                .Where(p =>
                    p.FirstName.ToLower().Contains(term) ||
                    p.LastName.ToLower().Contains(term) ||
                    (p.Phone != null && p.Phone.Contains(term)) ||
                    (p.Email != null && p.Email.ToLower().Contains(term)))
                .ToListAsync();

            return Ok(patients);
        }

        // =========================================================
        // GET: api/patients/{id}
        // =========================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var patient = await _db.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
                return NotFound($"Patient {id} not found.");

            return Ok(patient);
        }

        // =========================================================
        // POST: api/patients  -> Create patient
        // =========================================================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Patient p)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _db.Patients.Add(p);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = p.Id }, p);
        }

        // =========================================================
        // PUT: api/patients/{id} -> Update patient
        // =========================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Patient updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var patient = await _db.Patients.FindAsync(id);
            if (patient == null)
                return NotFound($"Patient {id} not found.");

            patient.FirstName = updated.FirstName;
            patient.LastName = updated.LastName;
            patient.DateOfBirth = updated.DateOfBirth;
            patient.Phone = updated.Phone;
            patient.Email = updated.Email;
            patient.Address = updated.Address;

            await _db.SaveChangesAsync();

            return Ok(patient);
        }

        // =========================================================
        // DELETE: api/patients/{id} -> Delete patient
        // =========================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _db.Patients.FindAsync(id);
            if (patient == null)
                return NotFound($"Patient {id} not found.");

            var hasAppointments = await _db.Appointments.AnyAsync(a => a.PatientId == id);
            if (hasAppointments)
            {
                return Conflict("Cannot delete patient with existing appointments.");
            }

            _db.Patients.Remove(patient);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // =========================================================
        // GET: api/patients/{patientId}/notes -> List Notes
        // =========================================================
        [HttpGet("{patientId}/notes")]
        public async Task<IActionResult> GetNotesForPatient(int patientId)
        {
            var exists = await _db.Patients.AnyAsync(p => p.Id == patientId);
            if (!exists)
                return NotFound($"Patient {patientId} not found.");

            var notes = await _db.PatientNotes
                .Where(n => n.PatientId == patientId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notes);
        }

        // =========================================================
        // POST: api/patients/{patientId}/notes -> Add Note
        // =========================================================
        [HttpPost("{patientId}/notes")]
        public async Task<IActionResult> AddNote(int patientId, [FromBody] PatientNote note)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var exists = await _db.Patients.AnyAsync(p => p.Id == patientId);
            if (!exists)
                return NotFound($"Patient {patientId} not found.");

            var currentUserId = GetCurrentUserId();

            note.PatientId = patientId;
            note.AuthorUserId = currentUserId;
            note.CreatedAt = DateTime.UtcNow;

            _db.PatientNotes.Add(note);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNotesForPatient),
                new { patientId = patientId },
                note);
        }

        // =========================================================
        // DELETE: api/patients/{patientId}/notes/{noteId} -> Delete Note
        // =========================================================
        [HttpDelete("{patientId}/notes/{noteId}")]
        public async Task<IActionResult> DeleteNote(int patientId, int noteId)
        {
            var patientExists = await _db.Patients.AnyAsync(p => p.Id == patientId);
            if (!patientExists)
                return NotFound($"Patient {patientId} not found.");

            var note = await _db.PatientNotes
                .FirstOrDefaultAsync(n => n.NoteId == noteId && n.PatientId == patientId);

            if (note == null)
                return NotFound($"Note {noteId} not found for patient {patientId}.");

            _db.PatientNotes.Remove(note);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // =========================================================
        // GET: api/patients/{id}/summary -> Full printable report
        // =========================================================
        [HttpGet("{id}/summary")]
        public async Task<IActionResult> GetSummary(int id)
        {
            var summary = await _db.Patients
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    patientId = p.Id,
                    fullName = p.FirstName + " " + p.LastName,
                    dateOfBirth = p.DateOfBirth,
                    phone = p.Phone,
                    email = p.Email,
                    address = p.Address,

                    appointments = p.Appointments
                        .OrderByDescending(a => a.StartsAt)
                        .Select(a => new
                        {
                            appointmentId = a.Id,
                            startsAt = a.StartsAt,
                            durationMinutes = a.DurationMinutes,
                            reason = a.Reason,
                            status = a.Status,
                            staffUserId = a.StaffUserId
                        })
                        .ToList(),

                    notes = p.Notes
                        .OrderByDescending(n => n.CreatedAt)
                        .Select(n => new
                        {
                            noteId = n.NoteId,
                            text = n.Text,
                            createdAt = n.CreatedAt,
                            authorUserId = n.AuthorUserId
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (summary == null)
                return NotFound($"Patient {id} not found.");

            return Ok(summary);
        }
    }
}
