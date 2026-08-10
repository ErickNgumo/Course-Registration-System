using Microsoft.EntityFrameworkCore;
using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Domain.Courses;

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Repositories;

/// <summary>Entity Framework implementation of course schedule reads.</summary>
public sealed class SchedulingRepository : ISchedulingRepository
{
    private readonly RegistrationDbContext _dbContext;

    public SchedulingRepository(RegistrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CourseSchedule>> GetCourseScheduleAsync(Guid courseId, CancellationToken cancellationToken) =>
        await _dbContext.CourseSchedules
            .AsNoTracking()
            .Where(schedule => schedule.CourseId == courseId)
            .OrderBy(schedule => schedule.DayOfWeek)
            .ThenBy(schedule => schedule.StartTime)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<CourseSchedule>>> GetSchedulesForCoursesAsync(
        IReadOnlyCollection<Guid> courseIds, CancellationToken cancellationToken)
    {
        var distinctIds = courseIds.Distinct().ToList();
        if (distinctIds.Count == 0)
        {
            return new Dictionary<Guid, IReadOnlyList<CourseSchedule>>();
        }

        var schedules = await _dbContext.CourseSchedules
            .AsNoTracking()
            .Where(schedule => distinctIds.Contains(schedule.CourseId))
            .ToListAsync(cancellationToken);

        return schedules
            .GroupBy(schedule => schedule.CourseId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<CourseSchedule>)group.OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime).ToList());
    }
}
