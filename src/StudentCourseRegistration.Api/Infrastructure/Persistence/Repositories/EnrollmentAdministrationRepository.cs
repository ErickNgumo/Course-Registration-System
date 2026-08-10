using Microsoft.EntityFrameworkCore;
using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Application.Common.Pagination;
using StudentCourseRegistration.Api.Domain.Enrollments;

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Repositories;

/// <summary>Entity Framework implementation of administrator enrollment management.</summary>
public sealed class EnrollmentAdministrationRepository : IEnrollmentAdministrationRepository
{
    private readonly RegistrationDbContext _dbContext;

    public EnrollmentAdministrationRepository(RegistrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<PagedResult<Enrollment>> SearchAsync(
        EnrollmentStatus? status,
        string? semester,
        Guid? courseId,
        Guid? studentId,
        string? sortBy,
        PageQuery page,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Enrollments.AsNoTracking();

        if (status is { } statusValue)
        {
            query = query.Where(enrollment => enrollment.Status == statusValue);
        }

        if (!string.IsNullOrWhiteSpace(semester))
        {
            var semesterCourseIds = await _dbContext.Courses
                .AsNoTracking()
                .Where(course => course.Semester == semester)
                .Select(course => course.Id)
                .ToListAsync(cancellationToken);

            query = query.Where(enrollment => semesterCourseIds.Contains(enrollment.CourseId));
        }

        if (courseId is { } courseValue)
        {
            query = query.Where(enrollment => enrollment.CourseId == courseValue);
        }

        if (studentId is { } studentValue)
        {
            query = query.Where(enrollment => enrollment.StudentId == studentValue);
        }

        query = ApplySorting(query, sortBy);

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResultFactory.Create(items, page.Page, page.PageSize, totalItems);
    }

    /// <inheritdoc />
    public Task<Enrollment?> FindByIdAsync(Guid enrollmentId, CancellationToken cancellationToken) =>
        _dbContext.Enrollments.SingleOrDefaultAsync(enrollment => enrollment.Id == enrollmentId, cancellationToken);

    /// <inheritdoc />
    public Task<Enrollment?> FindOldestWaitlistedAsync(Guid courseId, CancellationToken cancellationToken) =>
        _dbContext.Enrollments
            .OrderBy(enrollment => enrollment.RegisteredAt)
            .FirstOrDefaultAsync(enrollment =>
                enrollment.CourseId == courseId &&
                enrollment.Status == EnrollmentStatus.Waitlisted,
                cancellationToken);

    /// <inheritdoc />
    public async Task UpdateAsync(Enrollment enrollment, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.Enrollments.SingleOrDefaultAsync(e => e.Id == enrollment.Id, cancellationToken);
        if (existing is null)
        {
            return;
        }

        existing.Status = enrollment.Status;
        existing.DroppedAt = enrollment.DroppedAt;
        existing.FinalGrade = enrollment.FinalGrade;
        existing.UpdatedAt = enrollment.UpdatedAt;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<EnrollmentStatus, int>> CountByStatusAsync(CancellationToken cancellationToken)
    {
        var counts = await _dbContext.Enrollments
            .AsNoTracking()
            .GroupBy(enrollment => enrollment.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        var dictionary = Enum.GetValues<EnrollmentStatus>().ToDictionary(status => status, _ => 0);
        foreach (var count in counts)
        {
            dictionary[count.Status] = count.Count;
        }

        return dictionary;
    }

    /// <inheritdoc />
    public Task<int> CountRegisteredAsync(CancellationToken cancellationToken) =>
        _dbContext.Enrollments.CountAsync(enrollment => enrollment.Status == EnrollmentStatus.Registered, cancellationToken);

    /// <inheritdoc />
    public Task<int> CountWaitlistedAsync(CancellationToken cancellationToken) =>
        _dbContext.Enrollments.CountAsync(enrollment => enrollment.Status == EnrollmentStatus.Waitlisted, cancellationToken);

    /// <inheritdoc />
    public async Task<int> CountAvailableSeatsAsync(CancellationToken cancellationToken)
    {
        var activeCourses = await _dbContext.Courses
            .AsNoTracking()
            .Where(course => course.IsActive)
            .Select(course => new { course.Id, course.Capacity })
            .ToListAsync(cancellationToken);

        if (activeCourses.Count == 0)
        {
            return 0;
        }

        var registeredByCourse = await _dbContext.Enrollments
            .AsNoTracking()
            .Where(enrollment => enrollment.Status == EnrollmentStatus.Registered)
            .GroupBy(enrollment => enrollment.CourseId)
            .Select(group => new { CourseId = group.Key, Registered = group.Count() })
            .ToDictionaryAsync(item => item.CourseId, item => item.Registered, cancellationToken);

        return activeCourses.Sum(course => Math.Max(0, course.Capacity - registeredByCourse.GetValueOrDefault(course.Id)));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CourseEnrollmentCount>> GetCourseEnrollmentCountsAsync(CancellationToken cancellationToken)
    {
        var courses = await _dbContext.Courses
            .AsNoTracking()
            .Select(course => new
            {
                course.Id,
                course.Code,
                course.Name,
                course.Credits,
                course.Capacity,
                course.Semester,
                course.IsActive
            })
            .ToListAsync(cancellationToken);

        var counts = await _dbContext.Enrollments
            .AsNoTracking()
            .GroupBy(enrollment => new { enrollment.CourseId, enrollment.Status })
            .Select(group => new { group.Key.CourseId, group.Key.Status, Count = group.Count() })
            .ToListAsync(cancellationToken);

        var byCourse = counts
            .GroupBy(item => item.CourseId)
            .ToDictionary(group => group.Key, group => group.ToDictionary(item => item.Status, item => item.Count));

        return courses
            .Select(course =>
            {
                var perStatus = byCourse.GetValueOrDefault(course.Id) ?? new Dictionary<EnrollmentStatus, int>();
                return new CourseEnrollmentCount(
                    course.Id, course.Code, course.Name, course.Credits, course.Capacity,
                    perStatus.GetValueOrDefault(EnrollmentStatus.Registered),
                    perStatus.GetValueOrDefault(EnrollmentStatus.Waitlisted),
                    course.Semester, course.IsActive);
            })
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StudentCredits>> GetRegisteredCreditDistributionAsync(
        string? semester, CancellationToken cancellationToken)
    {
        var query = _dbContext.Enrollments
            .AsNoTracking()
            .Where(enrollment => enrollment.Status == EnrollmentStatus.Registered);

        if (!string.IsNullOrWhiteSpace(semester))
        {
            query = query.Where(enrollment =>
                _dbContext.Courses.Any(course => course.Id == enrollment.CourseId && course.Semester == semester));
        }

        return await query
            .GroupBy(enrollment => enrollment.StudentId)
            .Select(group => new StudentCredits(group.Key, group.Sum(enrollment =>
                _dbContext.Courses.Where(course => course.Id == enrollment.CourseId).Sum(course => course.Credits))))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SemesterStatistic>> GetSemesterStatisticsAsync(CancellationToken cancellationToken)
    {
        var rows = await (from enrollment in _dbContext.Enrollments.AsNoTracking()
                          join course in _dbContext.Courses.AsNoTracking() on enrollment.CourseId equals course.Id
                          group enrollment by course.Semester into @group
                          select new
                          {
                              Semester = @group.Key,
                              Registered = @group.Count(e => e.Status == EnrollmentStatus.Registered),
                              Waitlisted = @group.Count(e => e.Status == EnrollmentStatus.Waitlisted),
                              Completed = @group.Count(e => e.Status == EnrollmentStatus.Completed),
                              Dropped = @group.Count(e => e.Status == EnrollmentStatus.Dropped)
                          })
                         .ToListAsync(cancellationToken);

        return rows
            .Select(row => new SemesterStatistic(row.Semester, row.Registered, row.Waitlisted, row.Completed, row.Dropped))
            .OrderBy(statistic => statistic.Semester)
            .ToList();
    }

    private static IQueryable<Enrollment> ApplySorting(IQueryable<Enrollment> query, string? sortBy)
    {
        return sortBy?.ToUpperInvariant() switch
        {
            "STATUS" => query.OrderByDescending(enrollment => enrollment.Status),
            "COURSEID" => query.OrderBy(enrollment => enrollment.CourseId),
            _ => query.OrderByDescending(enrollment => enrollment.RegisteredAt)
        };
    }
}
