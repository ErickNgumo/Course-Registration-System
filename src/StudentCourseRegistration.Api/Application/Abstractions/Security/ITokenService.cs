using StudentCourseRegistration.Api.Domain.Administrators;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Application.Abstractions.Security;

public interface ITokenService
{
    /// <summary>Creates an access token for the given student with the Student role.</summary>
    AccessToken CreateAccessToken(Student student);

    /// <summary>Creates an access token for the given student using the supplied role.</summary>
    AccessToken CreateAccessToken(Student student, string role);

    /// <summary>Creates an access token for the given administrator with the Administrator role.</summary>
    AccessToken CreateAccessToken(Administrator administrator);
}

/// <summary>An issued bearer access token and its expiry.</summary>
public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);
