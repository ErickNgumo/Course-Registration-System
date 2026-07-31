using StudentCourseRegistration.Api.Application.Auth;

namespace StudentCourseRegistration.Api.Api.Contracts.Auth;

public sealed record StudentResponse(
    Guid Id,
    string StudentNumber,
    string FirstName,
    string LastName,
    string Email)
{
    public static StudentResponse From(AuthenticatedStudent student) => new(
        student.Id,
        student.StudentNumber,
        student.FirstName,
        student.LastName,
        student.Email);
}
