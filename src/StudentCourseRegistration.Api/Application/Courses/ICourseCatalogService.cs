namespace StudentCourseRegistration.Api.Application.Courses;

public interface ICourseCatalogService
{
    Task<IReadOnlyList<CourseDto>> GetActiveCoursesAsync(CancellationToken cancellationToken);
    Task<CourseDto> GetActiveCourseAsync(Guid courseId, CancellationToken cancellationToken);
}
