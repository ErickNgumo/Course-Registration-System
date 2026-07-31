using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Application.Abstractions.Security;
using StudentCourseRegistration.Api.Application.Common.Exceptions;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Application.Auth;

/// <summary>Coordinates student authentication use cases and account-status policy.</summary>
public sealed class AuthenticationService : IAuthenticationService
{
    private readonly IStudentRepository _students;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IStudentRepository students,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ILogger<AuthenticationService> logger)
    {
        _students = students;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToUpperInvariant();
        var student = await _students.FindByEmailAsync(email, cancellationToken);

        if (student is null || !_passwordHasher.Verify(student, command.Password))
        {
            _logger.LogWarning("Authentication failed due to invalid credentials.");
            throw new AuthenticationException();
        }

        if (student.Status != StudentStatus.Active)
        {
            _logger.LogWarning("Authentication failed because the student account is not active.");
            throw new ForbiddenException("This student account is not active.");
        }

        var token = _tokenService.CreateAccessToken(student);
        var expiresIn = Math.Max(0, (int)Math.Floor((token.ExpiresAt - DateTimeOffset.UtcNow).TotalSeconds));
        return new LoginResult(token.Value, "Bearer", expiresIn, MapStudent(student));
    }

    public async Task<AuthenticatedStudent> GetCurrentStudentAsync(
        Guid studentId,
        CancellationToken cancellationToken)
    {
        var student = await _students.FindByIdAsync(studentId, cancellationToken)
            ?? throw new NotFoundException("student");

        if (student.Status != StudentStatus.Active)
        {
            throw new ForbiddenException("This student account is not active.");
        }

        return MapStudent(student);
    }

    private static AuthenticatedStudent MapStudent(Student student) => new(
        student.Id,
        student.StudentNumber,
        student.FirstName,
        student.LastName,
        student.Email);
}
