namespace StudentCourseRegistration.Api.Application.Auth;

public interface IAuthenticationService
{
    Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken);
    Task<AuthenticatedStudent> GetCurrentStudentAsync(Guid studentId, CancellationToken cancellationToken);
}
