using StudentCourseRegistration.Api.Application.Courses;

namespace StudentCourseRegistration.Api.Api.Contracts.Courses;

public sealed record CourseResponse(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    int Credits,
    int Capacity,
    string Semester)
{
    public static CourseResponse From(CourseDto course) => new(
        course.Id,
        course.Code,
        course.Name,
        course.Description,
        course.Credits,
        course.Capacity,
        course.Semester);
}
