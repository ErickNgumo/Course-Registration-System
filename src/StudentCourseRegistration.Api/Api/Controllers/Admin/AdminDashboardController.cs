using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentCourseRegistration.Api.Api.Contracts.Admin;
using StudentCourseRegistration.Api.Api.Security;
using StudentCourseRegistration.Api.Application.Admin;
using StudentCourseRegistration.Api.Application.Security;

namespace StudentCourseRegistration.Api.Api.Controllers.Admin;

/// <summary>Administrator dashboard endpoint.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/dashboard")]
[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed class AdminDashboardController : ControllerBase
{
    private readonly IAdministrationService _administrationService;

    public AdminDashboardController(IAdministrationService administrationService)
    {
        _administrationService = administrationService;
    }

    /// <summary>Returns the consolidated administration dashboard.</summary>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The dashboard aggregates.</returns>
    /// <response code="200">The dashboard was returned.</response>
    /// <response code="401">No authenticated administrator.</response>
    /// <response code="403">The caller is not an administrator.</response>
    [HttpGet]
    [ProducesResponseType(typeof(AdministratorDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdministratorDashboardResponse>> GetDashboard(CancellationToken cancellationToken)
    {
        var dashboard = await _administrationService.GetDashboardAsync(cancellationToken);
        return Ok(AdministratorDashboardResponse.From(dashboard));
    }
}
