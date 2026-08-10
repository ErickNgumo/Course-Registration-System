using Microsoft.EntityFrameworkCore;
using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Application.Common.Pagination;
using StudentCourseRegistration.Api.Domain.Courses;
using StudentCourseRegistration.Api.Domain.Enrollments;

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Repositories;

/// <summary>Entity Framework implementation of administrator course management.</summary>
public sealed class CourseAdministrationRepository : ICourseAdministrationRepository
{
    private readonly RegistrationDbContext _dbContext;

    public CourseAdministrationRepository(RegistrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<PagedResult<Course>> SearchAsync(
        string? search, string? sortBy, PageQuery page, CancellationToken cancellationToken)
    {
        var query = _dbContext.Courses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(course => course.Code.Contains(term) || course.Name.Contains(term));
        }

        query = sortBy?.ToUpperInvariant() switch
        {
            "NAME" => query.OrderBy(course => course.Name),
            "ISACTIVE" => query.OrderByDescending(course => course.IsActive).ThenBy(course => course.Code),
            _ => query.OrderBy(course => course.Code)
        };

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query.Skip(page.Skip).Take(page.PageSize).ToListAsync(cancellationToken);
        return PagedResultFactory.Create(items, page.Page, page.PageSize, totalItems);
    }

    /// <inheritdoc />
    public Task<Course?> FindByIdAsync(Guid courseId, CancellationToken cancellationToken) =>
        _dbContext.Courses.SingleOrDefaultAsync(course => course.Id == courseId, cancellationToken);

    /// <inheritdoc />
    public Task<bool> CodeExistsAsync(string code, Guid? excludingCourseId, CancellationToken cancellationToken) =>
        _dbContext.Courses.AnyAsync(course =>
            course.Code == code &&
            (excludingCourseId == null || course.Id != excludingCourseId),
            cancellationToken);

    /// <inheritdoc />
    public Task<int> CountAsync(CancellationToken cancellationToken) =>
        _dbContext.Courses.CountAsync(cancellationToken);

    /// <inheritdoc />
    public Task<int> CountActiveAsync(CancellationToken cancellationToken) =>
        _dbContext.Courses.CountAsync(course => course.IsActive, cancellationToken);

    /// <inheritdoc />
    public Task<int> CountActiveEnrollmentsAsync(Guid courseId, CancellationToken cancellationToken) =>
        _dbContext.Enrollments.CountAsync(enrollment =>
            enrollment.CourseId == courseId &&
            (enrollment.Status == EnrollmentStatus.Registered || enrollment.Status == EnrollmentStatus.Waitlisted),
            cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Guid>> GetPrerequisiteCourseIdsAsync(Guid courseId, CancellationToken cancellationToken) =>
        await _dbContext.CoursePrerequisites
            .Where(prerequisite => prerequisite.CourseId == courseId)
            .Select(prerequisite => prerequisite.PrerequisiteCourseId)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<CourseSchedule>> GetScheduleAsync(Guid courseId, CancellationToken cancellationToken) =>
        await _dbContext.CourseSchedules
            .Where(schedule => schedule.CourseId == courseId)
            .OrderBy(schedule => schedule.DayOfWeek)
            .ThenBy(schedule => schedule.StartTime)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<Course> AddAsync(Course course, CancellationToken cancellationToken)
    {
        await _dbContext.Courses.AddAsync(course, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return course;
    }

    /// <inheritdoc />
    public async Task<Course> UpdateAsync(Course course, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.Courses.SingleOrDefaultAsync(c => c.Id == course.Id, cancellationToken);
        if (existing is null)
        {
            throw new InvalidOperationException("Course not found.");
        }

        existing.Code = course.Code;
        existing.Name = course.Name;
        existing.Description = course.Description;
        existing.Credits = course.Credits;
        existing.Capacity = course.Capacity;
        existing.Semester = course.Semester;
        existing.IsActive = course.IsActive;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.Courses.SingleOrDefaultAsync(c => c.Id == courseId, cancellationToken);
        if (existing is null)
        {
            return;
        }

        _dbContext.Courses.Remove(existing);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task ReplaceScheduleAsync(
        Guid courseId, IReadOnlyCollection<CourseSchedule> schedule, CancellationToken cancellationToken)
    {
        await _dbContext.CourseSchedules
            .Where(slot => slot.CourseId == courseId)
            .ExecuteDeleteAsync(cancellationToken);

        if (schedule.Count > 0)
        {
            await _dbContext.CourseSchedules.AddRangeAsync(schedule, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task ReplacePrerequisitesAsync(
        Guid courseId, IReadOnlyCollection<Guid> prerequisiteCourseIds, CancellationToken cancellationToken)
    {
        await _dbContext.CoursePrerequisites
            .Where(prerequisite => prerequisite.CourseId == courseId)
            .ExecuteDeleteAsync(cancellationToken);

        if (prerequisiteCourseIds.Count > 0)
        {
            var now = DateTimeOffset.UtcNow;
            var additions = prerequisiteCourseIds
                .Select(prerequisiteId => new CoursePrerequisite
                {
                    CourseId = courseId,
                    PrerequisiteCourseId = prerequisiteId,
                    CreatedAt = now
                });
            await _dbContext.CoursePrerequisites.AddRangeAsync(additions, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
