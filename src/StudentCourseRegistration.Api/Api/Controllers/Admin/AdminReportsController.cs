using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentCourseRegistration.Api.Api.Contracts.Admin;
using StudentCourseRegistration.Api.Application.Admin;
using StudentCourseRegistration.Api.Application.Security;

namespace StudentCourseRegistration.Api.Api.Controllers.Admin;

/// <summary>Administrator reporting endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/reports")]
[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed class AdminReportsController : ControllerBase
{
    private readonly IReportingService _reportingService;

    public AdminReportsController(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    /// <summary>Returns the course enrollment report with capacity utilization.</summary>
    /// <param name="cancellationToken">The request cancellation token.</param>
    [HttpGet("enrollment")]
    [ProducesResponseType(typeof(CourseEnrollmentReport), StatusCodes.Status200OK)]
    public async Task<ActionResult<CourseEnrollmentReport>> GetEnrollment(CancellationToken cancellationToken)
    {
        var report = await _reportingService.GetCourseEnrollmentAsync(cancellationToken);
        return Ok(report);
    }

    /// <summary>Returns the students-by-status report.</summary>
    /// <param name="cancellationToken">The request cancellation token.</param>
    [HttpGet("students")]
    [ProducesResponseType(typeof(StudentsByStatusReport), StatusCodes.Status200OK)]
    public async Task<ActionResult<StudentsByStatusReport>> GetStudents(CancellationToken cancellationToken)
    {
        var report = await _reportingService.GetStudentsByStatusAsync(cancellationToken);
        return Ok(report);
    }

    /// <summary>Returns the courses that currently have available seats.</summary>
    /// <param name="cancellationToken">The request cancellation token.</param>
    [HttpGet("courses")]
    [ProducesResponseType(typeof(AvailableSeatsReport), StatusCodes.Status200OK)]
    public async Task<ActionResult<AvailableSeatsReport>> GetAvailableSeats(CancellationToken cancellationToken)
    {
        var report = await _reportingService.GetAvailableSeatsAsync(cancellationToken);
        return Ok(report);
    }

    /// <summary>Returns the courses that currently have waitlists.</summary>
    [HttpGet("waitlist")]
    [ProducesResponseType(typeof(WaitlistReport), StatusCodes.Status200OK)]
    public async Task<ActionResult<WaitlistReport>> GetWaitlist(CancellationToken cancellationToken)
    {
        var report = await _reportingService.GetWaitlistReportAsync(cancellationToken);
        return Ok(report);
    }
}