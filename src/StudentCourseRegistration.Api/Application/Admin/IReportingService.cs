namespace StudentCourseRegistration.Api.Application.Admin;

/// <summary>Assembles analytical reports from repository aggregates.</summary>
public interface IReportingService
{
    /// <summary>Returns the students-by-status report.</summary>
    Task<StudentsByStatusReport> GetStudentsByStatusAsync(CancellationToken cancellationToken);

    /// <summary>Returns the course enrollment report with capacity utilization.</summary>
    Task<CourseEnrollmentReport> GetCourseEnrollmentAsync(CancellationToken cancellationToken);

    /// <summary>Returns the courses that currently have waitlists.</summary>
    Task<WaitlistReport> GetWaitlistReportAsync(CancellationToken cancellationToken);

    /// <summary>Returns the courses that currently have available seats.</summary>
    Task<AvailableSeatsReport> GetAvailableSeatsAsync(CancellationToken cancellationToken);

    /// <summary>Returns the top enrolled courses by registered count.</summary>
    Task<TopCoursesReport> GetTopEnrolledCoursesAsync(int count, CancellationToken cancellationToken);

    /// <summary>Returns the registered-credit distribution across students.</summary>
    Task<CreditDistributionReport> GetCreditDistributionAsync(CancellationToken cancellationToken);

    /// <summary>Returns the semester statistics report.</summary>
    Task<SemesterStatisticsReport> GetSemesterStatisticsAsync(CancellationToken cancellationToken);
}
