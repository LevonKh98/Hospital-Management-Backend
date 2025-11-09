using System;

namespace BackendApi.Dtos.Auth
{
    public record LoginResponse(string Token, string Name, string Role);
}
