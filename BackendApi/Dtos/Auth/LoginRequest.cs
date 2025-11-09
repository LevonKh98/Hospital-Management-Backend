using System;

namespace BackendApi.Dtos.Auth
{
    public record LoginRequest(string Username, string Password);
}
