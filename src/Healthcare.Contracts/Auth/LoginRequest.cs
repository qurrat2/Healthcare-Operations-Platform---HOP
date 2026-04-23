using System.ComponentModel.DataAnnotations;

namespace Healthcare.Contracts.Auth;

public sealed record LoginRequest(
    [property: Required, StringLength(100, MinimumLength = 1)] string Username,
    [property: Required, StringLength(100, MinimumLength = 1)] string Password);
