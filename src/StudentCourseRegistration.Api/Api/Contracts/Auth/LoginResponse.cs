using StudentCourseRegistration.Api.Application.Auth;

namespace StudentCourseRegistration.Api.Api.Contracts.Auth;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    StudentResponse Student)
{
    public static LoginResponse From(LoginResult result) => new(
        result.AccessToken,
        result.TokenType,
        result.ExpiresIn,
        StudentResponse.From(result.Student));
}
