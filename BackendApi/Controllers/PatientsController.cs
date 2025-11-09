using BackendApi.Data;
using BackendApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]   // -> /api/patients
    public class PatientsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public PatientsController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Get all patients.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var patients = await _db.Patients
                .AsNoTracking()
                .ToListAsync();

            return Ok(patients);
        }

        /// <summary>
        /// Create a new patient.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Patient p)
        {
            _db.Patients.Add(p);
            await _db.SaveChangesAsync();

            // we can return the created object
            return CreatedAtAction(nameof(GetAll), new { id = p.Id }, p);
        }
    }
}
