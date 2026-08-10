using StudentCourseRegistration.Api.Domain.Enrollments;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Application.Admin;

/// <summary>Students grouped by status.</summary>
public sealed record StudentsByStatusReport(IReadOnlyList<StatusCount> Statuses);

/// <summary>A status and the number of students in it.</summary>
public sealed record StatusCount(StudentStatus Status, int Count);

/// <summary>Course enrollment counts and capacity utilization.</summary>
public sealed record CourseEnrollmentReport(IReadOnlyList<CourseEnrollmentDetail> Courses);

/// <summary>A course and its enrollment/capacity details.</summary>
public sealed record CourseEnrollmentDetail(
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
    bool IsActive);

/// <summary>Courses that are full and the size of their waitlists.</summary>
public sealed record WaitlistReport(IReadOnlyList<WaitlistDetail> Waitlists);

/// <summary>A course and the number of students waiting for a seat.</summary>
public sealed record WaitlistDetail(Guid CourseId, string Code, string Name, string Semester, int Waitlisted);

/// <summary>Courses with at least one available seat.</summary>
public sealed record AvailableSeatsReport(IReadOnlyList<CourseEnrollmentDetail> Courses);

/// <summary>The courses with the most registered students.</summary>
public sealed record TopCoursesReport(IReadOnlyList<CourseEnrollmentDetail> Courses);

/// <summary>Credit distribution across active students.</summary>
public sealed record CreditDistributionReport(IReadOnlyList<CreditBucket> Buckets);

/// <summary>A credit range and how many students fall within it.</summary>
public sealed record CreditBucket(string Range, int Students);

/// <summary>Semester enrollment statistics.</summary>
public sealed record SemesterStatisticsReport(IReadOnlyList<SemesterStatisticDto> Semesters);
