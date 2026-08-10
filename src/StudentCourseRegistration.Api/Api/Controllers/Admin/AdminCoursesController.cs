using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentCourseRegistration.Api.Api.Contracts.Admin;
using StudentCourseRegistration.Api.Api.Contracts.Common;
using StudentCourseRegistration.Api.Api.Security;
using StudentCourseRegistration.Api.Application.Admin;
using StudentCourseRegistration.Api.Application.Common.Pagination;
using StudentCourseRegistration.Api.Application.Security;

namespace StudentCourseRegistration.Api.Api.Controllers.Admin;

/// <summary>Administrator course management endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/courses")]
[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed class AdminCoursesController : ControllerBase
{
    private readonly ICourseAdministrationService _courseAdministrationService;
    private readonly ICurrentUser _currentUser;

    public AdminCoursesController(ICourseAdministrationService courseAdministrationService, ICurrentUser currentUser)
    {
        _courseAdministrationService = courseAdministrationService;
        _currentUser = currentUser;
    }

    /// <summary>Returns a paged view of courses.</summary>
    /// <param name="search">Optional course code or name search text.</param>
    /// <param name="sortBy">Optional sort key.</param>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CourseAdministrationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<CourseAdministrationResponse>>> List(
        string? search, string? sortBy, int page, int pageSize, CancellationToken cancellationToken)
    {
        var result = await _courseAdministrationService.ListAsync(
            search, sortBy, new PageQuery { Page = page, PageSize = pageSize }, cancellationToken);
        return Ok(PagedResponse.From(result, CourseAdministrationResponse.From));
    }

    /// <summary>Creates a new course.</summary>
    /// <param name="request">The course to create.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    [HttpPost]
    [ProducesResponseType(typeof(CourseAdministrationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CourseAdministrationResponse>> Create(
        SaveCourseRequest request, CancellationToken cancellationToken)
    {
        var course = await _courseAdministrationService.CreateAsync(
            _currentUser.UserId, MapCreate(request), cancellationToken);
        var response = CourseAdministrationResponse.From(course);
        return CreatedAtAction(nameof(List), new { version = "1.0" }, response);
    }

    /// <summary>Updates an existing course's editable fields, schedules, and prerequisites.</summary>
    /// <param name="id">The course identifier.</param>
    /// <param name="request">The updated course.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CourseAdministrationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CourseAdministrationResponse>> Update(
        Guid id, SaveCourseRequest request, CancellationToken cancellationToken)
    {
        var course = await _courseAdministrationService.UpdateAsync(
            _currentUser.UserId, id, MapUpdate(request), cancellationToken);
        return Ok(CourseAdministrationResponse.From(course));
    }

    /// <summary>Permanently deletes a course that has no active enrollments.</summary>
    /// <param name="id">The course identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _courseAdministrationService.DeleteAsync(_currentUser.UserId, id, cancellationToken);
        return NoContent();
    }

    /// <summary>Activates a course.</summary>
    /// <param name="id">The course identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(CourseAdministrationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseAdministrationResponse>> Activate(
        Guid id, CancellationToken cancellationToken)
    {
        var course = await _courseAdministrationService.ActivateAsync(_currentUser.UserId, id, cancellationToken);
        return Ok(CourseAdministrationResponse.From(course));
    }

    /// <summary>Deactivates a course.</summary>
    /// <param name="id">The course identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(CourseAdministrationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseAdministrationResponse>> Deactivate(
        Guid id, CancellationToken cancellationToken)
    {
        var course = await _courseAdministrationService.DeactivateAsync(_currentUser.UserId, id, cancellationToken);
        return Ok(CourseAdministrationResponse.From(course));
    }

    private static CreateCourseCommand MapCreate(SaveCourseRequest request) => new(
        request.Code, request.Name, request.Description, request.Credits, request.Capacity, request.Semester,
        request.Schedules.Select(MapSchedule).ToList(), request.PrerequisiteCourseIds);

    private static UpdateCourseCommand MapUpdate(SaveCourseRequest request) => new(
        request.Code, request.Name, request.Description, request.Credits, request.Capacity, request.Semester,
        request.Schedules.Select(MapSchedule).ToList(), request.PrerequisiteCourseIds);

    private static CourseScheduleInput MapSchedule(ScheduleInput input) => new(input.DayOfWeek, input.StartTime, input.EndTime);
}
