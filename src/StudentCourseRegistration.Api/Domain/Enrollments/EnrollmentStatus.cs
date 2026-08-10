namespace StudentCourseRegistration.Api.Domain.Enrollments;

/// <summary>The lifecycle state of a student's enrollment in a course.</summary>
public enum EnrollmentStatus
{
    /// <summary>The student holds an active seat in the course.</summary>
    Registered = 1,

    /// <summary>The student is queued for a seat when one becomes available.</summary>
    Waitlisted = 2,

    /// <summary>The student has released their seat or left the waitlist.</summary>
    Dropped = 3,

    /// <summary>The student finished the course and received a final grade.</summary>
    Completed = 4
}
