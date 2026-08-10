using Microsoft.EntityFrameworkCore;
using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Application.Common.Pagination;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Repositories;

/// <summary>Entity Framework implementation of administrator student management.</summary>
public sealed class StudentAdministrationRepository : IStudentAdministrationRepository
{
    private readonly RegistrationDbContext _dbContext;

    public StudentAdministrationRepository(RegistrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<PagedResult<Student>> SearchAsync(
        StudentStatus? status,
        string? search,
        string? sortBy,
        PageQuery page,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Students.AsNoTracking();

        if (status is { } statusValue)
        {
            query = query.Where(student => student.Status == statusValue);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(student =>
                student.StudentNumber.Contains(term) ||
                student.Email.Contains(term) ||
                student.FirstName.Contains(term) ||
                student.LastName.Contains(term) ||
                (student.FirstName + " " + student.LastName).Contains(term));
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
    public Task<Student?> FindByIdAsync(Guid studentId, CancellationToken cancellationToken) =>
        _dbContext.Students.SingleOrDefaultAsync(student => student.Id == studentId, cancellationToken);

    /// <inheritdoc />
    public async Task UpdateStatusAsync(Student student, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.Students.SingleOrDefaultAsync(s => s.Id == student.Id, cancellationToken);
        if (existing is null)
        {
            return;
        }

        existing.Status = student.Status;
        existing.UpdatedAt = student.UpdatedAt;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<StudentStatus, int>> CountByStatusAsync(CancellationToken cancellationToken)
    {
        var counts = await _dbContext.Students
            .AsNoTracking()
            .GroupBy(student => student.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        var dictionary = Enum.GetValues<StudentStatus>().ToDictionary(status => status, _ => 0);
        foreach (var count in counts)
        {
            dictionary[count.Status] = count.Count;
        }

        return dictionary;
    }

    /// <inheritdoc />
    public Task<int> CountAsync(CancellationToken cancellationToken) =>
        _dbContext.Students.CountAsync(cancellationToken);

    private static IQueryable<Student> ApplySorting(IQueryable<Student> query, string? sortBy)
    {
        return sortBy?.ToUpperInvariant() switch
        {
            "STUDENTNUMBER" => query.OrderBy(student => student.StudentNumber),
            "EMAIL" => query.OrderBy(student => student.Email),
            "LASTNAME" => query.OrderBy(student => student.LastName).ThenBy(student => student.FirstName),
            _ => query.OrderBy(student => student.FirstName).ThenBy(student => student.LastName)
        };
    }
}
