using StudentCourseRegistration.Api.Domain.Administrators;

namespace StudentCourseRegistration.Api.Application.Abstractions.Persistence;

/// <summary>Persistence boundary for administrator account reads.</summary>
public interface IAdministratorRepository
{
    /// <summary>Finds an active administrator by normalized email address.</summary>
    Task<Administrator?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    /// <summary>Finds an administrator by identifier.</summary>
    Task<Administrator?> FindByIdAsync(Guid administratorId, CancellationToken cancellationToken);

    /// <summary>Returns all administrators.</summary>
    Task<IReadOnlyList<Administrator>> GetAllAsync(CancellationToken cancellationToken);
}
