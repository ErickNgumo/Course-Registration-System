using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Application.Common.Pagination;

namespace StudentCourseRegistration.Api.Application.Admin;

/// <summary>Reads the administrative audit log through the audit repository.</summary>
public sealed class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditRepository;

    public AuditService(IAuditLogRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    /// <inheritdoc />
    public async Task<PagedResult<AuditLogDto>> SearchAsync(
        string? entity,
        string? action,
        Guid? administratorId,
        PageQuery page,
        CancellationToken cancellationToken)
    {
        var paged = await _auditRepository.SearchAsync(entity, action, administratorId, page, cancellationToken);
        var dtos = paged.Items.Select(MapAuditLog).ToList();
        return PagedResultFactory.Create(dtos, paged.Page, paged.PageSize, paged.TotalItems);
    }

    private static AuditLogDto MapAuditLog(Domain.Audit.AuditLog auditLog) => new(
        auditLog.Id,
        auditLog.AdministratorId,
        auditLog.Action,
        auditLog.Entity,
        auditLog.EntityId,
        auditLog.Timestamp,
        auditLog.OldValues,
        auditLog.NewValues);
}
