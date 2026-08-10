using System.ComponentModel.DataAnnotations;

namespace StudentCourseRegistration.Api.Api.Contracts.Enrollments;

/// <summary>The body of a course registration request.</summary>
public sealed record RegisterEnrollmentRequest
{
    /// <summary>The identifier of the course to register into.</summary>
    [Required]
    public Guid CourseId { get; init; }
}
