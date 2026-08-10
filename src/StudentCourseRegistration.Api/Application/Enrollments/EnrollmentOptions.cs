namespace StudentCourseRegistration.Api.Application.Enrollments;

/// <summary>Student registration policy thresholds sourced from configuration.</summary>
public sealed class EnrollmentOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "Enrollment";

    /// <summary>The maximum number of credits a student may register for in one semester.</summary>
    public int MaxSemesterCredits { get; set; } = 21;

    /// <summary>When true, a full course places the student on the waitlist instead of rejecting the request.</summary>
    public bool WaitlistEnabled { get; set; } = true;
}
