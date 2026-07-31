using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Application.Abstractions.Persistence;

public interface IStudentRepository
{
    Task<Student?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
    Task<Student?> FindByIdAsync(Guid studentId, CancellationToken cancellationToken);
}
