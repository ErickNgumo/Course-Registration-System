using StudentCourseRegistration.Api.Application.Common.Pagination;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Application.Abstractions.Persistence;

/// <summary>Persistence boundary for administrator student management operations.</summary>
public interface IStudentAdministrationRepository
{
    /// <summary>Returns a paged view of students, optionally filtered by status and search text.</summary>
    Task<PagedResult<Student>> SearchAsync(
        StudentStatus? status,
        string? search,
        string? sortBy,
        PageQuery page,
        CancellationToken cancellationToken);

    /// <summary>Finds a single student by identifier, including profile and history reads.</summary>
    Task<Student?> FindByIdAsync(Guid studentId, CancellationToken cancellationToken);

    /// <summary>Persists a status change for the supplied student.</summary>
    Task UpdateStatusAsync(Student student, CancellationToken cancellationToken);

    /// <summary>Counts students grouped by status.</summary>
    Task<IReadOnlyDictionary<StudentStatus, int>> CountByStatusAsync(CancellationToken cancellationToken);

    /// <summary>Counts all students.</summary>
    Task<int> CountAsync(CancellationToken cancellationToken);
}
