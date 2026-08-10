using StudentCourseRegistration.Api.Domain.Enrollments;

namespace StudentCourseRegistration.Api.Application.Enrollments;

/// <summary>A consolidated view of the authenticated student's academic standing.</summary>
public sealed record DashboardDto(
    DashboardStudentDto Student,
    int CurrentSemesterCredits,
    int MaxSemesterCredits,
    IReadOnlyList<EnrollmentDto> RegisteredCourses,
    IReadOnlyList<EnrollmentDto> WaitlistedCourses,
    IReadOnlyList<EnrollmentDto> CompletedCourses);

/// <summary>The identifying information shown on the dashboard.</summary>
public sealed record DashboardStudentDto(
    Guid Id,
    string StudentNumber,
    string FirstName,
    string LastName,
    string Email);
