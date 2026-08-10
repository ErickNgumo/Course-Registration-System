using Microsoft.EntityFrameworkCore;
using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Domain.Courses;

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Repositories;

/// <summary>Entity Framework implementation of course catalog reads.</summary>
public sealed class CourseRepository : ICourseRepository
{
    private readonly RegistrationDbContext _dbContext;

    public CourseRepository(RegistrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Course>> GetActiveCoursesAsync(CancellationToken cancellationToken) =>
        await _dbContext.Courses
            .AsNoTracking()
            .Where(course => course.IsActive)
            .OrderBy(course => course.Code)
            .ToListAsync(cancellationToken);

    public Task<Course?> FindByIdAsync(Guid courseId, CancellationToken cancellationToken) =>
        _dbContext.Courses
            .AsNoTracking()
            .SingleOrDefaultAsync(course => course.Id == courseId, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, Course>> FindByIdsAsync(
        IReadOnlyCollection<Guid> courseIds, CancellationToken cancellationToken)
    {
        var distinctIds = courseIds.Distinct().ToList();
        if (distinctIds.Count == 0)
        {
            return new Dictionary<Guid, Course>();
        }

        var courses = await _dbContext.Courses
            .AsNoTracking()
            .Where(course => distinctIds.Contains(course.Id))
            .ToListAsync(cancellationToken);

        return courses.ToDictionary(course => course.Id);
    }
}
