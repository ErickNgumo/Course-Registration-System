using StudentCourseRegistration.Api.Domain.Enrollments;

namespace StudentCourseRegistration.Api.Application.Enrollments;

/// <summary>The student-facing view of a single enrollment.</summary>
public sealed record EnrollmentDto(
    Guid Id,
    Guid CourseId,
    string CourseCode,
    string CourseName,
    string Semester,
    int Credits,
    EnrollmentStatus Status,
    DateTimeOffset RegisteredAt,
    DateTimeOffset? DroppedAt,
    string? FinalGrade);
