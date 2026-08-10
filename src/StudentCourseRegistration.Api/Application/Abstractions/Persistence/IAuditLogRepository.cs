using StudentCourseRegistration.Api.Application.Common.Pagination;
using StudentCourseRegistration.Api.Domain.Audit;

namespace StudentCourseRegistration.Api.Application.Abstractions.Persistence;

/// <summary>Persistence boundary for audit log records.</summary>
public interface IAuditLogRepository
{
    /// <summary>Records an audit entry. Returns the persisted record.</summary>
    Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken);

    /// <summary>Returns a paged view of audit records, optionally filtered by entity and action.</summary>
    Task<PagedResult<AuditLog>> SearchAsync(
        string? entity,
        string? action,
        Guid? administratorId,
        PageQuery page,
        CancellationToken cancellationToken);
}
