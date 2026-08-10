using StudentCourseRegistration.Api.Domain.Enrollments;

namespace StudentCourseRegistration.Api.Application.Enrollments;

/// <summary>Coordinates student-facing course registration use cases and their business rules.</summary>
public interface IEnrollmentService
{
    /// <summary>Registers the current student into a course, placing them on the waitlist when the course is full.</summary>
    Task<EnrollmentDto> RegisterAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken);

    /// <summary>Drops an enrollment owned by the current student and promotes the next waitlisted student.</summary>
    Task DropAsync(Guid studentId, Guid enrollmentId, CancellationToken cancellationToken);

    /// <summary>Returns the current student's active and completed enrollments.</summary>
    Task<IReadOnlyList<EnrollmentDto>> GetEnrollmentsAsync(Guid studentId, CancellationToken cancellationToken);

    /// <summary>Returns a consolidated view of the current student's academic standing.</summary>
    Task<DashboardDto> GetDashboardAsync(Guid studentId, CancellationToken cancellationToken);
}
