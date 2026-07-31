using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentCourseRegistration.Api.Api.Contracts.Auth;
using StudentCourseRegistration.Api.Api.Security;
using StudentCourseRegistration.Api.Application.Auth;

namespace StudentCourseRegistration.Api.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ICurrentUser _currentUser;

    public AuthController(IAuthenticationService authenticationService, ICurrentUser currentUser)
    {
        _authenticationService = authenticationService;
        _currentUser = currentUser;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authenticationService.LoginAsync(
            new LoginCommand(request.Email, request.Password),
            cancellationToken);

        return Ok(LoginResponse.From(result));
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(StudentResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<StudentResponse>> Me(CancellationToken cancellationToken)
    {
        var student = await _authenticationService.GetCurrentStudentAsync(
            _currentUser.StudentId,
            cancellationToken);

        return Ok(StudentResponse.From(student));
    }
}
