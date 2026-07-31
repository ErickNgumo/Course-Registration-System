namespace StudentCourseRegistration.Api.Domain.Courses;

/// <summary>Represents a course that may be published in the student course catalog.</summary>
public sealed class Course
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Credits { get; set; }
    public int Capacity { get; set; }
    public string Semester { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
