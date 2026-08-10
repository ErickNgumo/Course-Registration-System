using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentCourseRegistration.Api.Api.Contracts.Admin;
using StudentCourseRegistration.Api.Api.Contracts.Common;
using StudentCourseRegistration.Api.Api.Security;
using StudentCourseRegistration.Api.Application.Admin;
using StudentCourseRegistration.Api.Application.Common.Pagination;
using StudentCourseRegistration.Api.Application.Security;
using StudentCourseRegistration.Api.Domain.Enrollments;

namespace StudentCourseRegistration.Api.Api.Controllers.Admin;

/// <summary>Administrator enrollment management endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/enrollments")]
[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed class AdminEnrollmentsController : ControllerBase
{
    private readonly IEnrollmentAdministrationService _enrollmentAdministrationService;
    private readonly ICurrentUser _currentUser;

    public AdminEnrollmentsController(
        IEnrollmentAdministrationService enrollmentAdministrationService, ICurrentUser currentUser)
    {
        _enrollmentAdministrationService = enrollmentAdministrationService;
        _currentUser = currentUser;
    }

    /// <summary>Returns a paged, filtered view of enrollments.</summary>
    /// <param name="status">Optional status filter.</param>
    /// <param name="semester">Optional semester filter.</param>
    /// <param name="courseId">Optional course filter.</param>
    /// <param name="studentId">Optional student filter.</param>
    /// <param name="sortBy">Optional sort key.</param>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<EnrollmentAdministrationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<EnrollmentAdministrationResponse>>> List(
        string? status,
        string? semester,
        Guid? courseId,
        Guid? studentId,
        string? sortBy,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var statusFilter = ParseStatus(status);
        var result = await _enrollmentAdministrationService.ListAsync(
            statusFilter, semester, courseId, studentId, sortBy,
            new PageQuery { Page = page, PageSize = pageSize }, cancellationToken);
        return Ok(PagedResponse.From(result, EnrollmentAdministrationResponse.From));
    }

    /// <summary>Force-drops an enrollment and promotes the next waitlisted student.</summary>
    /// <param name="id">The enrollment identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Drop(Guid id, CancellationToken cancellationToken)
    {
        await _enrollmentAdministrationService.DropAsync(_currentUser.UserId, id, cancellationToken);
        return NoContent();
    }

    /// <summary>Approves the promotion of the next waitlisted student for a course.</summary>
    /// <param name="courseId">The course identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    [HttpPost("waitlist/{courseId:guid}/promote")]
    [ProducesResponseType(typeof(EnrollmentAdministrationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnrollmentAdministrationResponse>> ApproveWaitlistPromotion(
        Guid courseId, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentAdministrationService.ApproveWaitlistPromotionAsync(
            _currentUser.UserId, courseId, cancellationToken);
        return Ok(EnrollmentAdministrationResponse.From(enrollment));
    }

    /// <summary>Assigns or updates an enrollment's final grade and marks it completed.</summary>
    /// <param name="id">The enrollment identifier.</param>
    /// <param name="request">The grade to assign.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    [HttpPatch("{id:guid}/grade")]
    [ProducesResponseType(typeof(EnrollmentAdministrationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<EnrollmentAdministrationResponse>> AssignGrade(
        Guid id, AssignGradeRequest request, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentAdministrationService.AssignGradeAsync(
            _currentUser.UserId, id, request.FinalGrade, cancellationToken);
        return Ok(EnrollmentAdministrationResponse.From(enrollment));
    }

    private static EnrollmentStatus? ParseStatus(string? status)
    {
        return string.IsNullOrWhiteSpace(status)
            ? null
            : Enum.TryParse<EnrollmentStatus>(status, ignoreCase: true, out var parsed) ? parsed : null;
    }
}
