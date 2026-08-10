using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentCourseRegistration.Api.Api.Contracts.Dashboard;
using StudentCourseRegistration.Api.Api.Security;
using StudentCourseRegistration.Api.Application.Enrollments;

namespace StudentCourseRegistration.Api.Api.Controllers;

/// <summary>Student academic dashboard endpoint.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/dashboard")]
[Authorize]
public sealed class DashboardController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly ICurrentUser _currentUser;

    public DashboardController(IEnrollmentService enrollmentService, ICurrentUser currentUser)
    {
        _enrollmentService = enrollmentService;
        _currentUser = currentUser;
    }

    /// <summary>Returns the consolidated academic dashboard for the current student.</summary>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The dashboard.</returns>
    /// <response code="200">The dashboard was returned.</response>
    /// <response code="401">No authenticated student.</response>
    /// <response code="404">The student was not found.</response>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DashboardResponse>> GetDashboard(CancellationToken cancellationToken)
    {
        var dashboard = await _enrollmentService.GetDashboardAsync(
            _currentUser.StudentId, cancellationToken);
        return Ok(DashboardResponse.From(dashboard));
    }
}
