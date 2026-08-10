using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Application.Admin;

/// <summary>Builds analytical reports from repository aggregates.</summary>
public sealed class ReportingService : IReportingService
{
    private readonly IStudentAdministrationRepository _students;
    private readonly IEnrollmentAdministrationRepository _enrollments;

    public ReportingService(
        IStudentAdministrationRepository students,
        IEnrollmentAdministrationRepository enrollments)
    {
        _students = students;
        _enrollments = enrollments;
    }

    /// <inheritdoc />
    public async Task<StudentsByStatusReport> GetStudentsByStatusAsync(CancellationToken cancellationToken)
    {
        var counts = await _students.CountByStatusAsync(cancellationToken);
        return new StudentsByStatusReport(counts.Select(MapStatus).ToList());
    }

    /// <inheritdoc />
    public async Task<CourseEnrollmentReport> GetCourseEnrollmentAsync(CancellationToken cancellationToken)
    {
        var rows = await _enrollments.GetCourseEnrollmentCountsAsync(cancellationToken);
        return new CourseEnrollmentReport(rows.Select(MapCourseDetail).ToList());
    }

    /// <inheritdoc />
    public async Task<WaitlistReport> GetWaitlistReportAsync(CancellationToken cancellationToken)
    {
        var rows = await _enrollments.GetCourseEnrollmentCountsAsync(cancellationToken);
        var waitlists = rows
            .Where(row => row.Waitlisted > 0)
            .Select(row => new WaitlistDetail(row.CourseId, row.Code, row.Name, row.Semester, row.Waitlisted))
            .OrderByDescending(detail => detail.Waitlisted)
            .ToList();
        return new WaitlistReport(waitlists);
    }

    /// <inheritdoc />
    public async Task<AvailableSeatsReport> GetAvailableSeatsAsync(CancellationToken cancellationToken)
    {
        var rows = await _enrollments.GetCourseEnrollmentCountsAsync(cancellationToken);
        var withSeats = rows
            .Where(row => row.IsActive && row.Registered < row.Capacity)
            .Select(MapCourseDetail)
            .OrderBy(detail => detail.AvailableSeats)
            .ToList();
        return new AvailableSeatsReport(withSeats);
    }

    /// <inheritdoc />
    public async Task<TopCoursesReport> GetTopEnrolledCoursesAsync(int count, CancellationToken cancellationToken)
    {
        var rows = await _enrollments.GetCourseEnrollmentCountsAsync(cancellationToken);
        var top = rows
            .OrderByDescending(row => row.Registered)
            .ThenBy(row => row.Code)
            .Take(count < 1 ? 10 : count)
            .Select(MapCourseDetail)
            .ToList();
        return new TopCoursesReport(top);
    }

    /// <inheritdoc />
    public async Task<CreditDistributionReport> GetCreditDistributionAsync(CancellationToken cancellationToken)
    {
        var rows = await _enrollments.GetRegisteredCreditDistributionAsync(semester: null, cancellationToken);

        var distribution = rows
            .GroupBy(row => BucketFor(row.Credits))
            .ToDictionary(group => group.Key, group => group.Count());

        var buckets = new[] { "0–7 credits", "8–15 credits", "16–21 credits", ">21 credits" };
        var results = buckets
            .Select(bucket => new CreditBucket(bucket, distribution.GetValueOrDefault(bucket)))
            .ToList();
        return new CreditDistributionReport(results);
    }

    /// <inheritdoc />
    public async Task<SemesterStatisticsReport> GetSemesterStatisticsAsync(CancellationToken cancellationToken)
    {
        var stats = await _enrollments.GetSemesterStatisticsAsync(cancellationToken);
        return new SemesterStatisticsReport(stats.Select(MapSemester).ToList());
    }

    private static string BucketFor(int credits) => credits switch
    {
        <= 7 => "0–7 credits",
        <= 15 => "8–15 credits",
        <= 21 => "16–21 credits",
        _ => ">21 credits"
    };

    private static StatusCount MapStatus(KeyValuePair<StudentStatus, int> count) => new(count.Key, count.Value);

    private static CourseEnrollmentDetail MapCourseDetail(CourseEnrollmentCount row)
    {
        var available = Math.Max(0, row.Capacity - row.Registered);
        var utilization = row.Capacity > 0
            ? Math.Round((double)row.Registered / row.Capacity * 100, 1)
            : 0;
        return new CourseEnrollmentDetail(
            row.CourseId, row.Code, row.Name, row.Credits, row.Capacity,
            row.Registered, row.Waitlisted, available, utilization, row.Semester, row.IsActive);
    }

    private static SemesterStatisticDto MapSemester(SemesterStatistic statistic) => new(
        statistic.Semester,
        statistic.Registered,
        statistic.Waitlisted,
        statistic.Completed,
        statistic.Dropped);
}
