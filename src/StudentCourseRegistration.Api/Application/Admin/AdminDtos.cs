using StudentCourseRegistration.Api.Domain.Courses;
using StudentCourseRegistration.Api.Domain.Enrollments;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Application.Admin;

/// <summary>The administrator-facing view of a course.</summary>
public sealed record AdminCourseDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    int Credits,
    int Capacity,
    string Semester,
    bool IsActive,
    int ActiveEnrollmentCount,
    IReadOnlyList<CourseScheduleDto> Schedules,
    IReadOnlyList<Guid> PrerequisiteCourseIds);

/// <summary>A weekly meeting slot for a course.</summary>
public sealed record CourseScheduleDto(Guid Id, DayOfWeek DayOfWeek, string StartTime, string EndTime);

/// <summary>The administrator-facing view of a student.</summary>
public sealed record AdminStudentDto(
    Guid Id,
    string StudentNumber,
    string FirstName,
    string LastName,
    string Email,
    StudentStatus Status);

/// <summary>The administrator-facing view of an enrollment, with student and course context.</summary>
public sealed record AdminEnrollmentDto(
    Guid Id,
    Guid StudentId,
    string StudentEmail,
    Guid CourseId,
    string CourseCode,
    string CourseName,
    string Semester,
    EnrollmentStatus Status,
    DateTimeOffset RegisteredAt,
    DateTimeOffset? DroppedAt,
    string? FinalGrade);

/// <summary>The administrator audit log view.</summary>
public sealed record AuditLogDto(
    Guid Id,
    Guid AdministratorId,
    string Action,
    string Entity,
    Guid EntityId,
    DateTimeOffset Timestamp,
    string? OldValues,
    string? NewValues);
