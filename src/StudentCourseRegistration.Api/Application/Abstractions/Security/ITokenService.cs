using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Application.Abstractions.Security;

public interface ITokenService
{
    AccessToken CreateAccessToken(Student student);
}

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);
