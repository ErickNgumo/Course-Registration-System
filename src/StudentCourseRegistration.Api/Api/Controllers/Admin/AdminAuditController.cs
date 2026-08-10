using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentCourseRegistration.Api.Api.Contracts.Common;
using StudentCourseRegistration.Api.Application.Admin;
using StudentCourseRegistration.Api.Application.Common.Pagination;
using StudentCourseRegistration.Api.Application.Security;

namespace StudentCourseRegistration.Api.Api.Controllers.Admin;

/// <summary>Administrator audit log endpoint.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/audit")]
[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed class AdminAuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AdminAuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    /// <summary>Returns a paged view of the audit log.</summary>
    /// <param name="entity">Optional entity name filter.</param>
    /// <param name="action">Optional action filter.</param>
    /// <param name="administratorId">Optional administrator filter.</param>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<AuditLogDto>>> List(
        string? entity,
        string? action,
        Guid? administratorId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var result = await _auditService.SearchAsync(
            entity, action, administratorId,
            new PageQuery { Page = page, PageSize = pageSize },
            cancellationToken);
        return Ok(PagedResponse.From(result));
    }
}