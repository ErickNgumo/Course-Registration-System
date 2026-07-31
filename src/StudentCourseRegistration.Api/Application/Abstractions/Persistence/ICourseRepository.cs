using StudentCourseRegistration.Api.Domain.Courses;

namespace StudentCourseRegistration.Api.Application.Abstractions.Persistence;

public interface ICourseRepository
{
    Task<IReadOnlyList<Course>> GetActiveCoursesAsync(CancellationToken cancellationToken);
    Task<Course?> FindByIdAsync(Guid courseId, CancellationToken cancellationToken);
}
