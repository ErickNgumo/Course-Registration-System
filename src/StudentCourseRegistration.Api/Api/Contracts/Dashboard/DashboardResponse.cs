using StudentCourseRegistration.Api.Api.Contracts.Enrollments;
using StudentCourseRegistration.Api.Application.Enrollments;

namespace StudentCourseRegistration.Api.Api.Contracts.Dashboard;

/// <summary>The HTTP view of the student dashboard.</summary>
public sealed record DashboardResponse(
    Guid StudentId,
    string StudentNumber,
    string FirstName,
    string LastName,
    string Email,
    int CurrentSemesterCredits,
    int MaxSemesterCredits,
    IReadOnlyList<EnrollmentResponse> RegisteredCourses,
    IReadOnlyList<EnrollmentResponse> WaitlistedCourses,
    IReadOnlyList<EnrollmentResponse> CompletedCourses)
{
    /// <summary>Maps the application DTO to the HTTP response.</summary>
    public static DashboardResponse From(DashboardDto dashboard) => new(
        dashboard.Student.Id,
        dashboard.Student.StudentNumber,
        dashboard.Student.FirstName,
        dashboard.Student.LastName,
        dashboard.Student.Email,
        dashboard.CurrentSemesterCredits,
        dashboard.MaxSemesterCredits,
        dashboard.RegisteredCourses.Select(EnrollmentResponse.From).ToList(),
        dashboard.WaitlistedCourses.Select(EnrollmentResponse.From).ToList(),
        dashboard.CompletedCourses.Select(EnrollmentResponse.From).ToList());
}
