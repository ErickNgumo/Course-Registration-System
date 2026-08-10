using StudentCourseRegistration.Api.Application.Admin;

namespace StudentCourseRegistration.Api.Api.Contracts.Admin;

/// <summary>The HTTP view of a successful administrator login.</summary>
public sealed record AdministratorLoginResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    AdministratorIdentity Administrator)
{
    /// <summary>Maps the application result to the HTTP response.</summary>
    public static AdministratorLoginResponse From(AdministratorLoginResult result) => new(
        result.AccessToken,
        result.TokenType,
        result.ExpiresIn,
        new AdministratorIdentity(
            result.Administrator.Id,
            result.Administrator.FirstName,
            result.Administrator.LastName,
            result.Administrator.Email));
}

/// <summary>The authenticated administrator identity returned at login.</summary>
public sealed record AdministratorIdentity(
    Guid Id,
    string FirstName,
    string LastName,
    string Email);
