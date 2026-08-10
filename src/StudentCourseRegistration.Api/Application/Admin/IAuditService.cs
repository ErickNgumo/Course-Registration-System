using StudentCourseRegistration.Api.Application.Common.Pagination;

namespace StudentCourseRegistration.Api.Application.Admin;

/// <summary>Provides read access to the administrative audit log.</summary>
public interface IAuditService
{
    /// <summary>Returns a paged view of audit records.</summary>
    Task<PagedResult<AuditLogDto>> SearchAsync(
        string? entity,
        string? action,
        Guid? administratorId,
        PageQuery page,
        CancellationToken cancellationToken);
}
