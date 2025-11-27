using BackendApi.Data;
using BackendApi.Dtos.Users;
using BackendApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]   // -> /api/users
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher<User> _hasher;

        public UsersController(AppDbContext db, IPasswordHasher<User> hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        // =========================================================
        // 1) GET /api/users  (Admin only) - list all users
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _db.Users
                .AsNoTracking()
                .Select(u => new UserResponse(
                    u.UserId,
                    u.FullName,
                    u.Email,
                    u.Username,
                    u.Role,
                    u.IsActive))
                .ToListAsync();

            return Ok(users);
        }

        // =========================================================
        // 2) GET /api/users/{id}  (Admin only) - single user
        // =========================================================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var dto = new UserResponse(
                user.UserId,
                user.FullName,
                user.Email,
                user.Username,
                user.Role,
                user.IsActive
            );

            return Ok(dto);
        }

        // =========================================================
        // 3) POST /api/users  (Admin only) - create new user
        // =========================================================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest("Username and Password are required.");
            }

            // check unique username
            var exists = await _db.Users.AnyAsync(u => u.Username == req.Username);
            if (exists)
            {
                return Conflict("Username already exists.");
            }

            // check unique email (since your model has unique index)
            var emailExists = await _db.Users.AnyAsync(u => u.Email == req.Email);
            if (emailExists)
            {
                return Conflict("Email already exists.");
            }

            var user = new User
            {
                FullName = req.FullName,
                Email = req.Email,
                Username = req.Username,
                Role = req.Role,
                IsActive = true
            };

            // hash the plain password
            user.PasswordHash = _hasher.HashPassword(user, req.Password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var resp = new UserResponse(
                user.UserId,
                user.FullName,
                user.Email,
                user.Username,
                user.Role,
                user.IsActive
            );

            return CreatedAtAction(nameof(GetById), new { id = user.UserId }, resp);
        }

        // =========================================================
        // 4) PUT /api/users/{id}  (Admin only) - update existing user
        //     - can change name, email, username, role, isActive
        //     - can ALSO reset password if provided
        // =========================================================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest req)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // check username uniqueness (if changed)
            if (!string.Equals(user.Username, req.Username, StringComparison.OrdinalIgnoreCase))
            {
                var usernameTaken = await _db.Users
                    .AnyAsync(u => u.Username == req.Username && u.UserId != id);
                if (usernameTaken)
                {
                    return Conflict("Username already exists.");
                }
            }

            // check email uniqueness (if changed)
            if (!string.Equals(user.Email, req.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailTaken = await _db.Users
                    .AnyAsync(u => u.Email == req.Email && u.UserId != id);
                if (emailTaken)
                {
                    return Conflict("Email already exists.");
                }
            }

            // update fields
            user.FullName = req.FullName;
            user.Email = req.Email;
            user.Username = req.Username;
            user.Role = req.Role;
            user.IsActive = req.IsActive;

            // reset password only if provided
            if (!string.IsNullOrWhiteSpace(req.Password))
            {
                user.PasswordHash = _hasher.HashPassword(user, req.Password);
            }

            await _db.SaveChangesAsync();

            var dto = new UserResponse(
                user.UserId,
                user.FullName,
                user.Email,
                user.Username,
                user.Role,
                user.IsActive
            );

            return Ok(dto);
        }

        // =========================================================
        // 5) PATCH /api/users/{id}/deactivate  (Admin only)
        //     - soft delete / disable account
        //     - after this, login won't work because login checks IsActive
        // =========================================================
        [HttpPatch("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!user.IsActive)
            {
                // already deactivated, we can return 204 or 200
                return NoContent();
            }

            user.IsActive = false;
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
