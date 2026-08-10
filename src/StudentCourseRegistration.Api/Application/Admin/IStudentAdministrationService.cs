using StudentCourseRegistration.Api.Application.Common.Pagination;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Application.Admin;

/// <summary>Owns all administrator student management business rules.</summary>
public interface IStudentAdministrationService
{
    /// <summary>Returns a paged, filtered, searched view of students.</summary>
    Task<PagedResult<AdminStudentDto>> ListAsync(
        StudentStatus? status, string? search, string? sortBy, PageQuery page, CancellationToken cancellationToken);

    /// <summary>Returns the student profile together with academic history.</summary>
    Task<StudentProfileDto> GetAsync(Guid studentId, CancellationToken cancellationToken);

    /// <summary>Changes a student's status.</summary>
    Task<AdminStudentDto> ChangeStatusAsync(
        Guid administratorId, Guid studentId, StudentStatus status, CancellationToken cancellationToken);
}

/// <summary>The administrator-facing student profile with academic history.</summary>
public sealed record StudentProfileDto(
    AdminStudentDto Student,
    IReadOnlyList<AdminEnrollmentDto> CurrentRegistrations,
    IReadOnlyList<AdminEnrollmentDto> CompletedCourses,
    IReadOnlyList<AdminEnrollmentDto> Waitlists,
    IReadOnlyList<AdminEnrollmentDto> History);

/// <summary>Command to change a student's status.</summary>
public sealed record ChangeStudentStatusCommand(StudentStatus Status);
