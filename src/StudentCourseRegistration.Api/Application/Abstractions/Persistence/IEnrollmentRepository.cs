using StudentCourseRegistration.Api.Domain.Enrollments;

namespace StudentCourseRegistration.Api.Application.Abstractions.Persistence;

/// <summary>Persistence boundary for enrollment records.</summary>
public interface IEnrollmentRepository
{
    /// <summary>Finds the active (Registered or Waitlisted) enrollment for a student and course, if any.</summary>
    Task<Enrollment?> FindActiveAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken);

    /// <summary>Returns all non-dropped enrollments for a student.</summary>
    Task<IReadOnlyList<Enrollment>> GetStudentEnrollmentsAsync(Guid studentId, bool includeDropped, CancellationToken cancellationToken);

    /// <summary>Counts the enrollments currently holding a seat in a course.</summary>
    Task<int> CountRegisteredAsync(Guid courseId, CancellationToken cancellationToken);

    /// <summary>Counts the students waiting for a seat in a course.</summary>
    Task<int> CountWaitlistedAsync(Guid courseId, CancellationToken cancellationToken);

    /// <summary>Returns the oldest waitlisted enrollment for a course, ordered by registration time.</summary>
    Task<Enrollment?> FindOldestWaitlistedAsync(Guid courseId, CancellationToken cancellationToken);

    /// <summary>Counts the credits earned by a student through active registrations in a semester.</summary>
    Task<int> SumRegisteredCreditsAsync(Guid studentId, string semester, CancellationToken cancellationToken);

    /// <summary>Persists a new or modified enrollment. Returns the persisted entity.</summary>
    Task<Enrollment> UpsertAsync(Enrollment enrollment, CancellationToken cancellationToken);

    /// <summary>Finds an enrollment by identifier, optionally restricted to a student.</summary>
    Task<Enrollment?> FindByIdAsync(Guid enrollmentId, Guid? studentId, CancellationToken cancellationToken);

    /// <summary>Returns the completed enrollment for a course for a student, if any.</summary>
    Task<Enrollment?> FindCompletedAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken);
}
