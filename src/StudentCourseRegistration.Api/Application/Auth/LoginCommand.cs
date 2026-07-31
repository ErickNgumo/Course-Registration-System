namespace StudentCourseRegistration.Api.Application.Auth;

public sealed record LoginCommand(string Email, string Password);
