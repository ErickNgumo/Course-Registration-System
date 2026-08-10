using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentCourseRegistration.Api.Api.Contracts.Admin;
using StudentCourseRegistration.Api.Api.Contracts.Common;
using StudentCourseRegistration.Api.Api.Security;
using StudentCourseRegistration.Api.Application.Admin;
using StudentCourseRegistration.Api.Application.Common.Pagination;
using StudentCourseRegistration.Api.Application.Security;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Api.Controllers.Admin;

/// <summary>Administrator student management endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/students")]
[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed class AdminStudentsController : ControllerBase
{
    private readonly IStudentAdministrationService _studentAdministrationService;
    private readonly ICurrentUser _currentUser;

    public AdminStudentsController(
        IStudentAdministrationService studentAdministrationService, ICurrentUser currentUser)
    {
        _studentAdministrationService = studentAdministrationService;
        _currentUser = currentUser;
    }

    /// <summary>Returns a paged, filtered, searched view of students.</summary>
    /// <param name="status">Optional status filter.</param>
    /// <param name="search">Optional student number, email, or name search text.</param>
    /// <param name="sortBy">Optional sort key.</param>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<StudentAdministrationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<StudentAdministrationResponse>>> List(
        string? status, string? search, string? sortBy, int page, int pageSize, CancellationToken cancellationToken)
    {
        var statusFilter = ParseStatus(status);
        var result = await _studentAdministrationService.ListAsync(
            statusFilter, search, sortBy, new PageQuery { Page = page, PageSize = pageSize }, cancellationToken);
        return Ok(PagedResponse.From(result, StudentAdministrationResponse.From));
    }

    /// <summary>Returns a student's profile and academic history.</summary>
    /// <param name="id">The student identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StudentProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentProfileResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var profile = await _studentAdministrationService.GetAsync(id, cancellationToken);
        return Ok(StudentProfileResponse.From(profile));
    }

    /// <summary>Changes a student's status.</summary>
    /// <param name="id">The student identifier.</param>
    /// <param name="request">The requested status.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(StudentAdministrationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentAdministrationResponse>> ChangeStatus(
        Guid id, ChangeStudentStatusRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<StudentStatus>(request.Status, ignoreCase: true, out var status))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid status",
                Detail = "Status must be Active, Suspended, or Inactive."
            });
        }

        var student = await _studentAdministrationService.ChangeStatusAsync(_currentUser.UserId, id, status, cancellationToken);
        return Ok(StudentAdministrationResponse.From(student));
    }

    private static StudentStatus? ParseStatus(string? status)
    {
        return string.IsNullOrWhiteSpace(status)
            ? null
            : Enum.TryParse<StudentStatus>(status, ignoreCase: true, out var parsed) ? parsed : null;
    }
}
