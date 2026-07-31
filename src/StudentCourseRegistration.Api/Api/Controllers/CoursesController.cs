using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentCourseRegistration.Api.Api.Contracts.Courses;
using StudentCourseRegistration.Api.Application.Courses;

namespace StudentCourseRegistration.Api.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/courses")]
[Authorize]
public sealed class CoursesController : ControllerBase
{
    private readonly ICourseCatalogService _courseCatalogService;

    public CoursesController(ICourseCatalogService courseCatalogService)
    {
        _courseCatalogService = courseCatalogService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CourseResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CourseResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var courses = await _courseCatalogService.GetActiveCoursesAsync(cancellationToken);
        return Ok(courses.Select(CourseResponse.From).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var course = await _courseCatalogService.GetActiveCourseAsync(id, cancellationToken);
        return Ok(CourseResponse.From(course));
    }
}
