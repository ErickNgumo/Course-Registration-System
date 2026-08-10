using Microsoft.EntityFrameworkCore;
using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Application.Common.Pagination;
using StudentCourseRegistration.Api.Domain.Audit;

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Repositories;

/// <summary>Entity Framework implementation of audit log persistence.</summary>
public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly RegistrationDbContext _dbContext;

    public AuditLogRepository(RegistrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken)
    {
        await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return auditLog;
    }

    /// <inheritdoc />
    public async Task<PagedResult<AuditLog>> SearchAsync(
        string? entity,
        string? action,
        Guid? administratorId,
        PageQuery page,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(entity))
        {
            query = query.Where(audit => audit.Entity == entity);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(audit => audit.Action == action);
        }

        if (administratorId is { } adminId)
        {
            query = query.Where(audit => audit.AdministratorId == adminId);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(audit => audit.Timestamp)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResultFactory.Create(items, page.Page, page.PageSize, totalItems);
    }
}
