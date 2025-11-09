using System;

namespace BackendApi.Dtos.Users
{
    /// <summary>
    /// What we return to the client when listing / getting users.
    /// No password, only safe fields.
    /// </summary>
    public record UserResponse(
        int UserId,
        string FullName,
        string Email,
        string Username,
        string Role,
        bool IsActive
    );
}
