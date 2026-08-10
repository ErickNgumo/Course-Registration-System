using Microsoft.EntityFrameworkCore;
using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Domain.Administrators;

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Repositories;

/// <summary>Entity Framework implementation of administrator account reads.</summary>
public sealed class AdministratorRepository : IAdministratorRepository
{
    private readonly RegistrationDbContext _dbContext;

    public AdministratorRepository(RegistrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public Task<Administrator?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        _dbContext.Administrators.SingleOrDefaultAsync(
            administrator => administrator.Email.ToUpper() == normalizedEmail,
            cancellationToken);

    /// <inheritdoc />
    public Task<Administrator?> FindByIdAsync(Guid administratorId, CancellationToken cancellationToken) =>
        _dbContext.Administrators.SingleOrDefaultAsync(
            administrator => administrator.Id == administratorId,
            cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Administrator>> GetAllAsync(CancellationToken cancellationToken) =>
        await _dbContext.Administrators
            .AsNoTracking()
            .OrderBy(administrator => administrator.LastName)
            .ThenBy(administrator => administrator.FirstName)
            .ToListAsync(cancellationToken);
}
