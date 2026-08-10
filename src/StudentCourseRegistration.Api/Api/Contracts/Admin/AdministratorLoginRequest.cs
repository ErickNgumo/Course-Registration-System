using System.ComponentModel.DataAnnotations;

namespace StudentCourseRegistration.Api.Api.Contracts.Admin;

/// <summary>The credentials submitted by an administrator.</summary>
public sealed record AdministratorLoginRequest
{
    /// <summary>The administrator email address.</summary>
    [Required]
    public string Email { get; init; } = string.Empty;

    /// <summary>The administrator password.</summary>
    [Required]
    public string Password { get; init; } = string.Empty;
}
