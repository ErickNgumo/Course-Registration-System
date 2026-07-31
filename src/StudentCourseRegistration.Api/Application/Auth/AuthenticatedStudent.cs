namespace StudentCourseRegistration.Api.Application.Auth;

public sealed record AuthenticatedStudent(
    Guid Id,
    string StudentNumber,
    string FirstName,
    string LastName,
    string Email);
