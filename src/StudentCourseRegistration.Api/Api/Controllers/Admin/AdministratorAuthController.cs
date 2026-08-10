using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentCourseRegistration.Api.Api.Contracts.Admin;
using StudentCourseRegistration.Api.Application.Admin;

namespace StudentCourseRegistration.Api.Api.Controllers.Admin;

/// <summary>Administrator authentication endpoint.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/auth")]
public sealed class AdministratorAuthController : ControllerBase
{
    private readonly IAdministratorAuthenticationService _administratorAuthenticationService;

    public AdministratorAuthController(IAdministratorAuthenticationService administratorAuthenticationService)
    {
        _administratorAuthenticationService = administratorAuthenticationService;
    }

    /// <summary>Authenticates an administrator and returns a signed JWT.</summary>
    /// <param name="request">The administrator credentials.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The administrator access token.</returns>
    /// <response code="200">Login succeeded.</response>
    /// <response code="401">The credentials were invalid.</response>
    /// <response code="403">The administrator account is not active.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AdministratorLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdministratorLoginResponse>> Login(
        AdministratorLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _administratorAuthenticationService.LoginAsync(
            new AdministratorLoginCommand(request.Email, request.Password),
            cancellationToken);

        return Ok(AdministratorLoginResponse.From(result));
    }
}
