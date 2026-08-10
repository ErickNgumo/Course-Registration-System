using StudentCourseRegistration.Api.Domain.Courses;
using StudentCourseRegistration.Api.Domain.Enrollments;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Application.Admin;

/// <summary>The administrator dashboard aggregates.</summary>
public sealed record AdministratorDashboardDto(
    int TotalStudents,
    int ActiveStudents,
    int SuspendedStudents,
    int TotalCourses,
    int ActiveCourses,
    int TotalRegistrations,
    int RegisteredStudents,
    int WaitlistedStudents,
    int AvailableSeats,
    IReadOnlyList<SemesterStatisticDto> SemesterStatistics);

/// <summary>A semester and its enrollment totals shown on the dashboard.</summary>
public sealed record SemesterStatisticDto(
    string Semester,
    int Registered,
    int Waitlisted,
    int Completed,
    int Dropped);
