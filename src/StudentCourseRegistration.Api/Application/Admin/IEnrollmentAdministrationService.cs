using StudentCourseRegistration.Api.Application.Common.Pagination;
using StudentCourseRegistration.Api.Domain.Enrollments;

namespace StudentCourseRegistration.Api.Application.Admin;

/// <summary>Owns all administrator enrollment management business rules.</summary>
public interface IEnrollmentAdministrationService
{
    /// <summary>Returns a paged, filtered view of enrollments.</summary>
    Task<PagedResult<AdminEnrollmentDto>> ListAsync(
        EnrollmentStatus? status,
        string? semester,
        Guid? courseId,
        Guid? studentId,
        string? sortBy,
        PageQuery page,
        CancellationToken cancellationToken);

    /// <summary>Force-drops an enrollment and promotes the next waitlisted student.</summary>
    Task DropAsync(Guid administratorId, Guid enrollmentId, CancellationToken cancellationToken);

    /// <summary>Approves the promotion of the next waitlisted student for a course.</summary>
    Task<AdminEnrollmentDto> ApproveWaitlistPromotionAsync(
        Guid administratorId, Guid courseId, CancellationToken cancellationToken);

    /// <summary>Assigns or updates a final grade and marks the enrollment completed.</summary>
    Task<AdminEnrollmentDto> AssignGradeAsync(
        Guid administratorId, Guid enrollmentId, string? finalGrade, CancellationToken cancellationToken);
}

/// <summary>Command to assign or update an enrollment's final grade.</summary>
public sealed record AssignGradeCommand(string? FinalGrade);
