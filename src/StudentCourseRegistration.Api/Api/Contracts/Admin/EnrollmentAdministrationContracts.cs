using StudentCourseRegistration.Api.Application.Admin;

namespace StudentCourseRegistration.Api.Api.Contracts.Admin;

/// <summary>The HTTP view of an administrable enrollment.</summary>
public sealed record EnrollmentAdministrationResponse(
    Guid Id,
    Guid StudentId,
    string StudentEmail,
    Guid CourseId,
    string CourseCode,
    string CourseName,
    string Semester,
    string Status,
    DateTimeOffset RegisteredAt,
    DateTimeOffset? DroppedAt,
    string? FinalGrade)
{
    public static EnrollmentAdministrationResponse From(AdminEnrollmentDto enrollment) => new(
        enrollment.Id,
        enrollment.StudentId,
        enrollment.StudentEmail,
        enrollment.CourseId,
        enrollment.CourseCode,
        enrollment.CourseName,
        enrollment.Semester,
        enrollment.Status.ToString(),
        enrollment.RegisteredAt,
        enrollment.DroppedAt,
        enrollment.FinalGrade);
}

/// <summary>The body of a grade assignment request.</summary>
public sealed record AssignGradeRequest(string? FinalGrade);
