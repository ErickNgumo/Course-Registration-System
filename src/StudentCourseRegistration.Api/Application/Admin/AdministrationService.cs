using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Application.Admin;

/// <summary>Assembles the administration dashboard from repository aggregates.</summary>
public sealed class AdministrationService : IAdministrationService
{
    private readonly IStudentAdministrationRepository _students;
    private readonly ICourseAdministrationRepository _courses;
    private readonly IEnrollmentAdministrationRepository _enrollments;

    public AdministrationService(
        IStudentAdministrationRepository students,
        ICourseAdministrationRepository courses,
        IEnrollmentAdministrationRepository enrollments)
    {
        _students = students;
        _courses = courses;
        _enrollments = enrollments;
    }

    /// <inheritdoc />
    public async Task<AdministratorDashboardDto> GetDashboardAsync(CancellationToken cancellationToken)
    {
        var statusCounts = await _students.CountByStatusAsync(cancellationToken);
        var totalCourses = await _courses.CountAsync(cancellationToken);
        var activeCourses = await _courses.CountActiveAsync(cancellationToken);
        var registered = await _enrollments.CountRegisteredAsync(cancellationToken);
        var waitlisted = await _enrollments.CountWaitlistedAsync(cancellationToken);
        var availableSeats = await _enrollments.CountAvailableSeatsAsync(cancellationToken);
        var semesterStats = await _enrollments.GetSemesterStatisticsAsync(cancellationToken);

        return new AdministratorDashboardDto(
            TotalStudents: statusCounts.Values.Sum(),
            ActiveStudents: statusCounts.GetValueOrDefault(StudentStatus.Active),
            SuspendedStudents: statusCounts.GetValueOrDefault(StudentStatus.Suspended),
            TotalCourses: totalCourses,
            ActiveCourses: activeCourses,
            TotalRegistrations: registered + waitlisted,
            RegisteredStudents: registered,
            WaitlistedStudents: waitlisted,
            AvailableSeats: availableSeats,
            SemesterStatistics: semesterStats.Select(MapSemester).ToList());
    }

    private static SemesterStatisticDto MapSemester(SemesterStatistic statistic) => new(
        statistic.Semester,
        statistic.Registered,
        statistic.Waitlisted,
        statistic.Completed,
        statistic.Dropped);
}
