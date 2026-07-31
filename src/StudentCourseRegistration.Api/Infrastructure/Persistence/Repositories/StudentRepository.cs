using Microsoft.EntityFrameworkCore;
using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Repositories;

/// <summary>Entity Framework implementation of student account reads.</summary>
public sealed class StudentRepository : IStudentRepository
{
    private readonly RegistrationDbContext _dbContext;

    public StudentRepository(RegistrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Student?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        _dbContext.Students.SingleOrDefaultAsync(
            student => student.Email.ToUpper() == normalizedEmail,
            cancellationToken);

    public Task<Student?> FindByIdAsync(Guid studentId, CancellationToken cancellationToken) =>
        _dbContext.Students.SingleOrDefaultAsync(student => student.Id == studentId, cancellationToken);
}
