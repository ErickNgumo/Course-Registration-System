using StudentCourseRegistration.Api.Domain.Administrators;

namespace StudentCourseRegistration.Api.Application.Admin;

/// <summary>Coordinates administrator authentication.</summary>
public interface IAdministratorAuthenticationService
{
    /// <summary>Authenticates an active administrator and returns a signed JWT.</summary>
    Task<AdministratorLoginResult> LoginAsync(AdministratorLoginCommand command, CancellationToken cancellationToken);
}

/// <summary>The credentials submitted by an administrator.</summary>
public sealed record AdministratorLoginCommand(string Email, string Password);

/// <summary>The result of a successful administrator login.</summary>
public sealed record AdministratorLoginResult(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    AuthenticatedAdministrator Administrator);

/// <summary>An authenticated administrator identity.</summary>
public sealed record AuthenticatedAdministrator(
    Guid Id,
    string FirstName,
    string LastName,
    string Email);
