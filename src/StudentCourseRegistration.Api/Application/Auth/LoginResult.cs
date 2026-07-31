namespace StudentCourseRegistration.Api.Application.Auth;

public sealed record LoginResult(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    AuthenticatedStudent Student);
