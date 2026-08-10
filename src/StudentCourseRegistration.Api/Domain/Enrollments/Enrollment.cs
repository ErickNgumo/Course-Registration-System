namespace StudentCourseRegistration.Api.Domain.Enrollments;

/// <summary>Represents a student's relationship with a single course over time.</summary>
public sealed class Enrollment
{
    /// <summary>The unique identifier of the enrollment.</summary>
    public Guid Id { get; set; }

    /// <summary>The student who owns the enrollment.</summary>
    public Guid StudentId { get; set; }

    /// <summary>The course the enrollment refers to.</summary>
    public Guid CourseId { get; set; }

    /// <summary>The current state of the enrollment.</summary>
    public EnrollmentStatus Status { get; set; }

    /// <summary>The moment the student was first registered or waitlisted.</summary>
    public DateTimeOffset RegisteredAt { get; set; }

    /// <summary>The moment the student dropped the course, if applicable.</summary>
    public DateTimeOffset? DroppedAt { get; set; }

    /// <summary>The final grade awarded when the course was completed, if applicable.</summary>
    public string? FinalGrade { get; set; }

    /// <summary>The moment the enrollment record was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The moment the enrollment record was last modified.</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
