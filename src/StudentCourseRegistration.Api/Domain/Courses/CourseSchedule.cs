namespace StudentCourseRegistration.Api.Domain.Courses;

/// <summary>A recurring weekly meeting time for a course.</summary>
public sealed class CourseSchedule
{
    /// <summary>The unique identifier of the schedule entry.</summary>
    public Guid Id { get; set; }

    /// <summary>The course this schedule belongs to.</summary>
    public Guid CourseId { get; set; }

    /// <summary>The day of the week the meeting occurs.</summary>
    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>The start time of the meeting.</summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>The end time of the meeting.</summary>
    public TimeOnly EndTime { get; set; }

    /// <summary>The moment the schedule record was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
