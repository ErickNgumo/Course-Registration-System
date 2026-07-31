using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Application.Common.Exceptions;
using StudentCourseRegistration.Api.Domain.Courses;

namespace StudentCourseRegistration.Api.Application.Courses;

/// <summary>Provides the student-facing view of courses currently available in the catalog.</summary>
public sealed class CourseCatalogService : ICourseCatalogService
{
    private readonly ICourseRepository _courses;

    public CourseCatalogService(ICourseRepository courses)
    {
        _courses = courses;
    }

    public async Task<IReadOnlyList<CourseDto>> GetActiveCoursesAsync(CancellationToken cancellationToken)
    {
        var courses = await _courses.GetActiveCoursesAsync(cancellationToken);
        return courses.Where(course => course.IsActive).Select(MapCourse).ToList();
    }

    public async Task<CourseDto> GetActiveCourseAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var course = await _courses.FindByIdAsync(courseId, cancellationToken);

        if (course is null || !course.IsActive)
        {
            throw new NotFoundException("course");
        }

        return MapCourse(course);
    }

    private static CourseDto MapCourse(Course course) => new(
        course.Id,
        course.Code,
        course.Name,
        course.Description,
        course.Credits,
        course.Capacity,
        course.Semester);
}
