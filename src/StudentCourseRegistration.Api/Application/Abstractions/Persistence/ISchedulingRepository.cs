using StudentCourseRegistration.Api.Domain.Courses;

namespace StudentCourseRegistration.Api.Application.Abstractions.Persistence;

/// <summary>Persistence boundary for course schedules and prerequisites catalog reads.</summary>
public interface ISchedulingRepository
{
    /// <summary>Retrieves the weekly meeting schedule for a course.</summary>
    Task<IReadOnlyList<CourseSchedule>> GetCourseScheduleAsync(Guid courseId, CancellationToken cancellationToken);

    /// <summary>Retrieves the weekly meeting schedules for a set of courses.</summary>
    Task<IReadOnlyDictionary<Guid, IReadOnlyList<CourseSchedule>>> GetSchedulesForCoursesAsync(
        IReadOnlyCollection<Guid> courseIds, CancellationToken cancellationToken);
}

/// <summary>Persistence boundary for course prerequisite definitions.</summary>
public interface IPrerequisiteRepository
{
    /// <summary>Returns the prerequisite course identifiers required before taking the specified course.</summary>
    Task<IReadOnlyList<Guid>> GetPrerequisiteCourseIdsAsync(Guid courseId, CancellationToken cancellationToken);
}
