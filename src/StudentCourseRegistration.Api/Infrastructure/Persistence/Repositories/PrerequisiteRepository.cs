using Microsoft.EntityFrameworkCore;
using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Domain.Courses;

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Repositories;

/// <summary>Entity Framework implementation of prerequisite catalog reads.</summary>
public sealed class PrerequisiteRepository : IPrerequisiteRepository
{
    private readonly RegistrationDbContext _dbContext;

    public PrerequisiteRepository(RegistrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Guid>> GetPrerequisiteCourseIdsAsync(Guid courseId, CancellationToken cancellationToken) =>
        await _dbContext.CoursePrerequisites
            .AsNoTracking()
            .Where(prerequisite => prerequisite.CourseId == courseId)
            .Select(prerequisite => prerequisite.PrerequisiteCourseId)
            .ToListAsync(cancellationToken);
}
