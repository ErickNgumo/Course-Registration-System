using StudentCourseRegistration.Api.Application.Common.Pagination;
using StudentCourseRegistration.Api.Domain.Courses;

namespace StudentCourseRegistration.Api.Application.Admin;

/// <summary>Owns all administrator course management business rules.</summary>
public interface ICourseAdministrationService
{
    /// <summary>Returns a paged view of courses.</summary>
    Task<PagedResult<AdminCourseDto>> ListAsync(string? search, string? sortBy, PageQuery page, CancellationToken cancellationToken);

    /// <summary>Fetches a single course by identifier.</summary>
    Task<AdminCourseDto> GetAsync(Guid courseId, CancellationToken cancellationToken);

    /// <summary>Creates a new course.</summary>
    Task<AdminCourseDto> CreateAsync(Guid administratorId, CreateCourseCommand command, CancellationToken cancellationToken);

    /// <summary>Updates an existing course's editable fields, schedule, and prerequisites.</summary>
    Task<AdminCourseDto> UpdateAsync(Guid administratorId, Guid courseId, UpdateCourseCommand command, CancellationToken cancellationToken);

    /// <summary>Permanently deletes a course that has no active enrollments.</summary>
    Task DeleteAsync(Guid administratorId, Guid courseId, CancellationToken cancellationToken);

    /// <summary>Activates a course.</summary>
    Task<AdminCourseDto> ActivateAsync(Guid administratorId, Guid courseId, CancellationToken cancellationToken);

    /// <summary>Deactivates a course.</summary>
    Task<AdminCourseDto> DeactivateAsync(Guid administratorId, Guid courseId, CancellationToken cancellationToken);
}

/// <summary>A weekly meeting slot supplied by an administrator.</summary>
public sealed record CourseScheduleInput(DayOfWeek DayOfWeek, string StartTime, string EndTime);

/// <summary>Command to create a course.</summary>
public sealed record CreateCourseCommand(
    string Code,
    string Name,
    string? Description,
    int Credits,
    int Capacity,
    string Semester,
    IReadOnlyList<CourseScheduleInput> Schedules,
    IReadOnlyList<Guid> PrerequisiteCourseIds);

/// <summary>Command to update a course.</summary>
public sealed record UpdateCourseCommand(
    string Code,
    string Name,
    string? Description,
    int Credits,
    int Capacity,
    string Semester,
    IReadOnlyList<CourseScheduleInput> Schedules,
    IReadOnlyList<Guid> PrerequisiteCourseIds);
