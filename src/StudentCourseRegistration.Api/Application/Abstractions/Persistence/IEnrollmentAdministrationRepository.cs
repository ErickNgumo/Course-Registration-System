using StudentCourseRegistration.Api.Application.Common.Pagination;
using StudentCourseRegistration.Api.Domain.Enrollments;

namespace StudentCourseRegistration.Api.Application.Abstractions.Persistence;

/// <summary>Persistence boundary for administrator enrollment management operations.</summary>
public interface IEnrollmentAdministrationRepository
{
    /// <summary>Returns a paged view of enrollments with optional filters.</summary>
    Task<PagedResult<Enrollment>> SearchAsync(
        EnrollmentStatus? status,
        string? semester,
        Guid? courseId,
        Guid? studentId,
        string? sortBy,
        PageQuery page,
        CancellationToken cancellationToken);

    /// <summary>Finds an enrollment by identifier.</summary>
    Task<Enrollment?> FindByIdAsync(Guid enrollmentId, CancellationToken cancellationToken);

    /// <summary>Returns the oldest waitlisted enrollment for a course, or null.</summary>
    Task<Enrollment?> FindOldestWaitlistedAsync(Guid courseId, CancellationToken cancellationToken);

    /// <summary>Applies status, grade, and timestamp updates to an enrollment.</summary>
    Task UpdateAsync(Enrollment enrollment, CancellationToken cancellationToken);

    /// <summary>Counts enrollments by status for the dashboard and reports.</summary>
    Task<IReadOnlyDictionary<EnrollmentStatus, int>> CountByStatusAsync(CancellationToken cancellationToken);

    /// <summary>Counts currently registered students across the catalog.</summary>
    Task<int> CountRegisteredAsync(CancellationToken cancellationToken);

    /// <summary>Counts currently waitlisted students across the catalog.</summary>
    Task<int> CountWaitlistedAsync(CancellationToken cancellationToken);

    /// <summary>Returns the number of available seats summed across active courses.</summary>
    Task<int> CountAvailableSeatsAsync(CancellationToken cancellationToken);

    /// <summary>Returns per-course enrollment counts for reports.</summary>
    Task<IReadOnlyList<CourseEnrollmentCount>> GetCourseEnrollmentCountsAsync(CancellationToken cancellationToken);

    /// <summary>Returns the total registered credits each student carries, grouped by student.</summary>
    Task<IReadOnlyList<StudentCredits>> GetRegisteredCreditDistributionAsync(string? semester, CancellationToken cancellationToken);

    /// <summary>Returns enrollment counts grouped by semester for the requested active statuses.</summary>
    Task<IReadOnlyList<SemesterStatistic>> GetSemesterStatisticsAsync(CancellationToken cancellationToken);
}

/// <summary>A course and its current registered and waitlisted enrollment counts.</summary>
public sealed record CourseEnrollmentCount(
    Guid CourseId,
    string Code,
    string Name,
    int Credits,
    int Capacity,
    int Registered,
    int Waitlisted,
    string Semester,
    bool IsActive);

/// <summary>A student and the total credits of their registered enrollments.</summary>
public sealed record StudentCredits(Guid StudentId, int Credits);

/// <summary>A semester and its registration/waitlist/completion totals.</summary>
public sealed record SemesterStatistic(
    string Semester,
    int Registered,
    int Waitlisted,
    int Completed,
    int Dropped);
