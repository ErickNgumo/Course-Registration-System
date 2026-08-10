using StudentCourseRegistration.Api.Application.Admin;

namespace StudentCourseRegistration.Api.Api.Contracts.Admin;

/// <summary>The HTTP view of the students-by-status report.</summary>
public sealed record StudentsByStatusResponse(IReadOnlyList<StatusCountResponse> Statuses)
{
    public static StudentsByStatusResponse From(StudentsByStatusReport report) => new(
        report.Statuses.Select(StatusCountResponse.From).ToList());
}

/// <summary>A status and its count.</summary>
public sealed record StatusCountResponse(string Status, int Count)
{
    public static StatusCountResponse From(StatusCount count) => new(count.Status.ToString(), count.Count);
}

/// <summary>The HTTP view of the course enrollment report.</summary>
public sealed record CourseEnrollmentResponse(IReadOnlyList<CourseEnrollmentDetailResponse> Courses)
{
    public static CourseEnrollmentResponse From(CourseEnrollmentReport report) => new(
        report.Courses.Select(CourseEnrollmentDetailResponse.From).ToList());
}

/// <summary>A course and its enrollment/capacity details.</summary>
public sealed record CourseEnrollmentDetailResponse(
    Guid CourseId,
    string Code,
    string Name,
    int Credits,
    int Capacity,
    int Registered,
    int Waitlisted,
    int AvailableSeats,
    double UtilizationPercent,
    string Semester,
    bool IsActive)
{
    public static CourseEnrollmentDetailResponse From(CourseEnrollmentDetail detail) => new(
        detail.CourseId, detail.Code, detail.Name, detail.Credits, detail.Capacity,
        detail.Registered, detail.Waitlisted, detail.AvailableSeats, detail.UtilizationPercent,
        detail.Semester, detail.IsActive);
}

/// <summary>The HTTP view of the waitlist report.</summary>
public sealed record WaitlistResponse(IReadOnlyList<WaitlistDetailResponse> Waitlists)
{
    public static WaitlistResponse From(WaitlistReport report) => new(
        report.Waitlists.Select(WaitlistDetailResponse.From).ToList());
}

/// <summary>A course and its waitlist size.</summary>
public sealed record WaitlistDetailResponse(Guid CourseId, string Code, string Name, string Semester, int Waitlisted)
{
    public static WaitlistDetailResponse From(WaitlistDetail detail) => new(
        detail.CourseId, detail.Code, detail.Name, detail.Semester, detail.Waitlisted);
}

/// <summary>The HTTP view of the courses with available seats report.</summary>
public sealed record AvailableSeatsResponse(IReadOnlyList<CourseEnrollmentDetailResponse> Courses)
{
    public static AvailableSeatsResponse From(AvailableSeatsReport report) => new(
        report.Courses.Select(CourseEnrollmentDetailResponse.From).ToList());
}

/// <summary>The HTTP view of the top enrolled courses report.</summary>
public sealed record TopCoursesResponse(IReadOnlyList<CourseEnrollmentDetailResponse> Courses)
{
    public static TopCoursesResponse From(TopCoursesReport report) => new(
        report.Courses.Select(CourseEnrollmentDetailResponse.From).ToList());
}

/// <summary>The HTTP view of the credit distribution report.</summary>
public sealed record CreditDistributionResponse(IReadOnlyList<CreditBucketResponse> Buckets)
{
    public static CreditDistributionResponse From(CreditDistributionReport report) => new(
        report.Buckets.Select(CreditBucketResponse.From).ToList());
}

/// <summary>A credit range and the student count within it.</summary>
public sealed record CreditBucketResponse(string Range, int Students)
{
    public static CreditBucketResponse From(CreditBucket bucket) => new(bucket.Range, bucket.Students);
}

/// <summary>The HTTP view of the semester statistics report.</summary>
public sealed record SemesterStatisticsResponse(IReadOnlyList<SemesterStatisticResponse> Semesters)
{
    public static SemesterStatisticsResponse From(SemesterStatisticsReport report) => new(
        report.Semesters.Select(SemesterStatisticResponse.From).ToList());
}

/// <summary>The HTTP view of an audit log entry.</summary>
public sealed record AuditLogResponse(
    Guid Id,
    Guid AdministratorId,
    string Action,
    string Entity,
    Guid EntityId,
    DateTimeOffset Timestamp,
    string? OldValues,
    string? NewValues)
{
    public static AuditLogResponse From(AuditLogDto auditLog) => new(
        auditLog.Id, auditLog.AdministratorId, auditLog.Action, auditLog.Entity, auditLog.EntityId,
        auditLog.Timestamp, auditLog.OldValues, auditLog.NewValues);
}
