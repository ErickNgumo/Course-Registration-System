namespace StudentCourseRegistration.Api.Application.Courses;

public sealed record CourseDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    int Credits,
    int Capacity,
    string Semester);
