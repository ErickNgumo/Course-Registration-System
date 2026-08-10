using StudentCourseRegistration.Api.Application.Admin;

namespace StudentCourseRegistration.Api.Api.Contracts.Admin;

/// <summary>The HTTP view of an administrable student.</summary>
public sealed record StudentAdministrationResponse(
    Guid Id,
    string StudentNumber,
    string FirstName,
    string LastName,
    string Email,
    string Status)
{
    public static StudentAdministrationResponse From(AdminStudentDto student) => new(
        student.Id,
        student.StudentNumber,
        student.FirstName,
        student.LastName,
        student.Email,
        student.Status.ToString());
}

/// <summary>The HTTP view of the administrator student profile.</summary>
public sealed record StudentProfileResponse(
    StudentAdministrationResponse Student,
    IReadOnlyList<EnrollmentAdministrationResponse> CurrentRegistrations,
    IReadOnlyList<EnrollmentAdministrationResponse> CompletedCourses,
    IReadOnlyList<EnrollmentAdministrationResponse> Waitlists,
    IReadOnlyList<EnrollmentAdministrationResponse> History)
{
    public static StudentProfileResponse From(StudentProfileDto profile) => new(
        StudentAdministrationResponse.From(profile.Student),
        profile.CurrentRegistrations.Select(EnrollmentAdministrationResponse.From).ToList(),
        profile.CompletedCourses.Select(EnrollmentAdministrationResponse.From).ToList(),
        profile.Waitlists.Select(EnrollmentAdministrationResponse.From).ToList(),
        profile.History.Select(EnrollmentAdministrationResponse.From).ToList());
}
