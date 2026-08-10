namespace StudentCourseRegistration.Api.Domain.Courses;

/// <summary>Declares that a course must be completed before another course may be taken.</summary>
public sealed class CoursePrerequisite
{
    /// <summary>The course that requires a prerequisite.</summary>
    public Guid CourseId { get; set; }

    /// <summary>The course that must be completed first.</summary>
    public Guid PrerequisiteCourseId { get; set; }

    /// <summary>The moment the prerequisite record was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
