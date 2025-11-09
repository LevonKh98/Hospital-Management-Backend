using System;

namespace BackendApi.Dtos.Users
{
    /// <summary>
    /// Used by admin to update a user.
    /// Password is optional — only hashed if provided.
    /// </summary>
    public record UpdateUserRequest(
        string FullName,
        string Email,
        string Username,
        string Role,
        bool IsActive,
        string? Password // optional
    );
}
