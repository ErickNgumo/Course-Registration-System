using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Application.Abstractions.Security;
using StudentCourseRegistration.Api.Application.Common.Exceptions;
using StudentCourseRegistration.Api.Domain.Administrators;

namespace StudentCourseRegistration.Api.Application.Admin;

/// <summary>Coordinates administrator authentication and account-state policy.</summary>
public sealed class AdministratorAuthenticationService : IAdministratorAuthenticationService
{
    private readonly IAdministratorRepository _administrators;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher<Administrator> _passwordHasher;
    private readonly ILogger<AdministratorAuthenticationService> _logger;

    public AdministratorAuthenticationService(
        IAdministratorRepository administrators,
        ITokenService tokenService,
        IPasswordHasher<Administrator> passwordHasher,
        ILogger<AdministratorAuthenticationService> logger)
    {
        _administrators = administrators;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AdministratorLoginResult> LoginAsync(
        AdministratorLoginCommand command, CancellationToken cancellationToken)
    {
        var normalizedEmail = command.Email.Trim().ToUpperInvariant();
        var administrator = await _administrators.FindByEmailAsync(normalizedEmail, cancellationToken);

        if (administrator is null || !_passwordHasher.Verify(administrator, command.Password))
        {
            _logger.LogWarning("Administrator authentication failed due to invalid credentials.");
            throw new AuthenticationException();
        }

        if (!administrator.IsActive)
        {
            _logger.LogWarning("Administrator authentication failed because the account is not active.");
            throw new ForbiddenException("This administrator account is not active.");
        }

        var token = _tokenService.CreateAccessToken(administrator);
        var expiresIn = Math.Max(0, (int)Math.Floor((token.ExpiresAt - DateTimeOffset.UtcNow).TotalSeconds));
        return new AdministratorLoginResult(token.Value, "Bearer", expiresIn, MapAdministrator(administrator));
    }

    private static AuthenticatedAdministrator MapAdministrator(Administrator administrator) => new(
        administrator.Id,
        administrator.FirstName,
        administrator.LastName,
        administrator.Email);
}
