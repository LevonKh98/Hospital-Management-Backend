using System;

namespace BackendApi.Dtos.Users
{
    /// <summary>
    /// DTO used by Admin to create a new user.
    /// Password is plain here and will be hashed before saving.
    /// </summary>
    public record CreateUserRequest(
        string FullName,
        string Email,
        string Username,
        string Role,    // "Admin" | "Doctor" | "Nurse" | "Receptionist"
        string Password
    );
}
