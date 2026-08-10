using StudentCourseRegistration.Api.Application.Enrollments;
using StudentCourseRegistration.Api.Domain.Enrollments;

namespace StudentCourseRegistration.Api.Api.Contracts.Enrollments;

/// <summary>The HTTP view of a student enrollment.</summary>
public sealed record EnrollmentResponse(
    Guid Id,
    Guid CourseId,
    string CourseCode,
    string CourseName,
    string Semester,
    int Credits,
    EnrollmentStatus Status,
    DateTimeOffset RegisteredAt,
    DateTimeOffset? DroppedAt,
    string? FinalGrade)
{
    /// <summary>Maps the application DTO to the HTTP response.</summary>
    public static EnrollmentResponse From(EnrollmentDto enrollment) => new(
        enrollment.Id,
        enrollment.CourseId,
        enrollment.CourseCode,
        enrollment.CourseName,
        enrollment.Semester,
        enrollment.Credits,
        enrollment.Status,
        enrollment.RegisteredAt,
        enrollment.DroppedAt,
        enrollment.FinalGrade);
}
