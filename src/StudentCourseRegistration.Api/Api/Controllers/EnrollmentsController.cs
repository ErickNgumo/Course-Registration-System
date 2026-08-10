using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentCourseRegistration.Api.Api.Contracts.Enrollments;
using StudentCourseRegistration.Api.Api.Security;
using StudentCourseRegistration.Api.Application.Enrollments;

namespace StudentCourseRegistration.Api.Api.Controllers;

/// <summary>Student-facing course registration endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/enrollments")]
[Authorize]
public sealed class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly ICurrentUser _currentUser;

    public EnrollmentsController(IEnrollmentService enrollmentService, ICurrentUser currentUser)
    {
        _enrollmentService = enrollmentService;
        _currentUser = currentUser;
    }

    /// <summary>Registers the current student into the specified course.</summary>
    /// <param name="request">The course to register into.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The created enrollment.</returns>
    /// <response code="201">The enrollment was created.</response>
    /// <response code="401">No authenticated student.</response>
    /// <response code="404">The course was not found.</response>
    /// <response code="409">The student is already enrolled.</response>
    /// <response code="422">A business rule prevented registration.</response>
    [HttpPost]
    [ProducesResponseType(typeof(EnrollmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<EnrollmentResponse>> Register(
        RegisterEnrollmentRequest request, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentService.RegisterAsync(
            _currentUser.StudentId, request.CourseId, cancellationToken);

        var response = EnrollmentResponse.From(enrollment);
        return CreatedAtAction(
            actionName: nameof(GetEnrollments),
            controllerName: null,
            routeValues: new { version = "1.0" },
            value: response);
    }

    /// <summary>Drops an enrollment owned by the current student.</summary>
    /// <param name="id">The enrollment identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <response code="204">The enrollment was dropped.</response>
    /// <response code="404">The enrollment was not found for the student.</response>
    /// <response code="409">The enrollment cannot be dropped.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Drop(Guid id, CancellationToken cancellationToken)
    {
        await _enrollmentService.DropAsync(_currentUser.StudentId, id, cancellationToken);
        return NoContent();
    }

    /// <summary>Returns the current student's enrollments.</summary>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The student's enrollments.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EnrollmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EnrollmentResponse>>> GetEnrollments(
        CancellationToken cancellationToken)
    {
        var enrollments = await _enrollmentService.GetEnrollmentsAsync(
            _currentUser.StudentId, cancellationToken);
        return Ok(enrollments.Select(EnrollmentResponse.From).ToList());
    }
}
