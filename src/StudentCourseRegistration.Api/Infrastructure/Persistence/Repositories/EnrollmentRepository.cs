using Microsoft.EntityFrameworkCore;
using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Domain.Courses;
using StudentCourseRegistration.Api.Domain.Enrollments;

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Repositories;

/// <summary>Entity Framework implementation of enrollment persistence.</summary>
public sealed class EnrollmentRepository : IEnrollmentRepository
{
    private readonly RegistrationDbContext _dbContext;

    public EnrollmentRepository(RegistrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public Task<Enrollment?> FindActiveAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken) =>
        _dbContext.Enrollments
            .FirstOrDefaultAsync(enrollment =>
                enrollment.StudentId == studentId &&
                enrollment.CourseId == courseId &&
                (enrollment.Status == EnrollmentStatus.Registered || enrollment.Status == EnrollmentStatus.Waitlisted),
                cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Enrollment>> GetStudentEnrollmentsAsync(
        Guid studentId, bool includeDropped, CancellationToken cancellationToken)
    {
        var query = _dbContext.Enrollments
            .AsNoTracking()
            .Where(enrollment => enrollment.StudentId == studentId);

        if (!includeDropped)
        {
            query = query.Where(enrollment => enrollment.Status != EnrollmentStatus.Dropped);
        }

        return await query
            .OrderBy(enrollment => enrollment.RegisteredAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<int> CountRegisteredAsync(Guid courseId, CancellationToken cancellationToken) =>
        _dbContext.Enrollments
            .CountAsync(enrollment =>
                enrollment.CourseId == courseId &&
                enrollment.Status == EnrollmentStatus.Registered,
                cancellationToken);

    /// <inheritdoc />
    public Task<int> CountWaitlistedAsync(Guid courseId, CancellationToken cancellationToken) =>
        _dbContext.Enrollments
            .CountAsync(enrollment =>
                enrollment.CourseId == courseId &&
                enrollment.Status == EnrollmentStatus.Waitlisted,
                cancellationToken);

    /// <inheritdoc />
    public Task<Enrollment?> FindOldestWaitlistedAsync(Guid courseId, CancellationToken cancellationToken) =>
        _dbContext.Enrollments
            .OrderBy(enrollment => enrollment.RegisteredAt)
            .FirstOrDefaultAsync(enrollment =>
                enrollment.CourseId == courseId &&
                enrollment.Status == EnrollmentStatus.Waitlisted,
                cancellationToken);

    /// <inheritdoc />
    public async Task<int> SumRegisteredCreditsAsync(
        Guid studentId, string semester, CancellationToken cancellationToken)
    {
        var credits = await _dbContext.Enrollments
            .AsNoTracking()
            .Where(enrollment =>
                enrollment.StudentId == studentId &&
                enrollment.Status == EnrollmentStatus.Registered)
            .Join(_dbContext.Courses,
                enrollment => enrollment.CourseId,
                course => course.Id,
                (enrollment, course) => new { enrollment, course })
            .Where(pair => pair.course.Semester == semester)
            .SumAsync(pair => (int?)pair.course.Credits, cancellationToken);
        return credits ?? 0;
    }

    /// <inheritdoc />
    public async Task<Enrollment> UpsertAsync(Enrollment enrollment, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.Enrollments.FindAsync([enrollment.Id], cancellationToken);
        if (existing is null)
        {
            _dbContext.Enrollments.Add(enrollment);
        }
        else
        {
            _dbContext.Entry(existing).CurrentValues.SetValues(enrollment);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return existing ?? enrollment;
    }

    /// <inheritdoc />
    public Task<Enrollment?> FindByIdAsync(Guid enrollmentId, Guid? studentId, CancellationToken cancellationToken) =>
        _dbContext.Enrollments.AsNoTracking()
            .FirstOrDefaultAsync(enrollment =>
                enrollment.Id == enrollmentId &&
                (studentId == null || enrollment.StudentId == studentId),
                cancellationToken);

    /// <inheritdoc />
    public Task<Enrollment?> FindCompletedAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken) =>
        _dbContext.Enrollments.AsNoTracking()
            .FirstOrDefaultAsync(enrollment =>
                enrollment.StudentId == studentId &&
                enrollment.CourseId == courseId &&
                enrollment.Status == EnrollmentStatus.Completed,
                cancellationToken);
}
