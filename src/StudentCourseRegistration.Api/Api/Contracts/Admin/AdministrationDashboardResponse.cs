using StudentCourseRegistration.Api.Application.Admin;

namespace StudentCourseRegistration.Api.Api.Contracts.Admin;

/// <summary>The HTTP view of the administrator dashboard.</summary>
public sealed record AdministratorDashboardResponse(
    int TotalStudents,
    int ActiveStudents,
    int SuspendedStudents,
    int TotalCourses,
    int ActiveCourses,
    int TotalRegistrations,
    int RegisteredStudents,
    int WaitlistedStudents,
    int AvailableSeats,
    IReadOnlyList<SemesterStatisticResponse> SemesterStatistics)
{
    public static AdministratorDashboardResponse From(AdministratorDashboardDto dashboard) => new(
        dashboard.TotalStudents,
        dashboard.ActiveStudents,
        dashboard.SuspendedStudents,
        dashboard.TotalCourses,
        dashboard.ActiveCourses,
        dashboard.TotalRegistrations,
        dashboard.RegisteredStudents,
        dashboard.WaitlistedStudents,
        dashboard.AvailableSeats,
        dashboard.SemesterStatistics.Select(SemesterStatisticResponse.From).ToList());
}

/// <summary>A semester and its enrollment totals shown on the dashboard.</summary>
public sealed record SemesterStatisticResponse(
    string Semester,
    int Registered,
    int Waitlisted,
    int Completed,
    int Dropped)
{
    public static SemesterStatisticResponse From(SemesterStatisticDto statistic) => new(
        statistic.Semester,
        statistic.Registered,
        statistic.Waitlisted,
        statistic.Completed,
        statistic.Dropped);
}
