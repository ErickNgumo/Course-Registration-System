using StudentCourseRegistration.Api.Application.Common.Pagination;
using StudentCourseRegistration.Api.Domain.Courses;
using StudentCourseRegistration.Api.Domain.Enrollments;

namespace StudentCourseRegistration.Api.Application.Abstractions.Persistence;

/// <summary>Persistence boundary for administrator course management operations.</summary>
public interface ICourseAdministrationRepository
{
    /// <summary>Returns a paged view of courses, optionally filtered by search text.</summary>
    Task<PagedResult<Course>> SearchAsync(
        string? search, string? sortBy, PageQuery page, CancellationToken cancellationToken);

    /// <summary>Finds a course by identifier (tracked-for-write variant with optional asNoTracking).</summary>
    Task<Course?> FindByIdAsync(Guid courseId, CancellationToken cancellationToken);

    /// <summary>Returns true when a course with the given code already exists.</summary>
    Task<bool> CodeExistsAsync(string code, Guid? excludingCourseId, CancellationToken cancellationToken);

    /// <summary>Counts all courses.</summary>
    Task<int> CountAsync(CancellationToken cancellationToken);

    /// <summary>Counts active courses.</summary>
    Task<int> CountActiveAsync(CancellationToken cancellationToken);

    /// <summary>Counts active (Registered or Waitlisted) enrollments for a course.</summary>
    Task<int> CountActiveEnrollmentsAsync(Guid courseId, CancellationToken cancellationToken);

    /// <summary>Returns the prerequisite course identifiers required by the course.</summary>
    Task<IReadOnlyList<Guid>> GetPrerequisiteCourseIdsAsync(Guid courseId, CancellationToken cancellationToken);

    /// <summary>Returns the schedule entries for a course.</summary>
    Task<IReadOnlyList<CourseSchedule>> GetScheduleAsync(Guid courseId, CancellationToken cancellationToken);

    /// <summary>Adds a new course and returns the persisted entity.</summary>
    Task<Course> AddAsync(Course course, CancellationToken cancellationToken);

    /// <summary>Applies updates to an existing course's editable fields.</summary>
    Task<Course> UpdateAsync(Course course, CancellationToken cancellationToken);

    /// <summary>Removes a course that has no active enrollments.</summary>
    Task DeleteAsync(Guid courseId, CancellationToken cancellationToken);

    /// <summary>Replaces the schedule entries for a course with the supplied set.</summary>
    Task ReplaceScheduleAsync(Guid courseId, IReadOnlyCollection<CourseSchedule> schedule, CancellationToken cancellationToken);

    /// <summary>Replaces the direct prerequisites for a course with the supplied set.</summary>
    Task ReplacePrerequisitesAsync(Guid courseId, IReadOnlyCollection<Guid> prerequisiteCourseIds, CancellationToken cancellationToken);
}
