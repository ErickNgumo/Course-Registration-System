using System.ComponentModel.DataAnnotations;

namespace StudentCourseRegistration.Api.Api.Contracts.Auth;

public sealed class LoginRequest
{
    [Required, EmailAddress, StringLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(256, MinimumLength = 1)]
    public string Password { get; init; } = string.Empty;
}
