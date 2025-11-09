using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackendApi.Data;
using BackendApi.Dtos.Auth;
using BackendApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BackendApi.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher<User> _hasher;
        private readonly IConfiguration _cfg;

        public AuthController(
            AppDbContext db,
            IPasswordHasher<User> hasher,
            IConfiguration cfg)
        {
            _db = db;
            _hasher = hasher;
            _cfg = cfg;
        }

        /// <summary>
        /// Login with username + password.
        /// Returns JWT if credentials are valid.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            // 1) find active user
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == req.Username && u.IsActive);
            if (user == null)
            {
                return Unauthorized();
            }

            // 2) verify password
            var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
            if (verify == PasswordVerificationResult.Failed)
            {
                return Unauthorized();
            }

            // 3) build JWT
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:SigningKey"]!));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new("name", user.FullName ?? user.Username),
                new(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _cfg["Jwt:Issuer"],
                audience: _cfg["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            var jwtString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new LoginResponse(jwtString, user.FullName ?? user.Username, user.Role));
        }
    }
}
