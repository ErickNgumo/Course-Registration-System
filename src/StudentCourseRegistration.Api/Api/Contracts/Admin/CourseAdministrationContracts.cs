using StudentCourseRegistration.Api.Application.Admin;

namespace StudentCourseRegistration.Api.Api.Contracts.Admin;

/// <summary>The HTTP view of an administrable course.</summary>
public sealed record CourseAdministrationResponse(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    int Credits,
    int Capacity,
    string Semester,
    bool IsActive,
    int ActiveEnrollmentCount,
    IReadOnlyList<ScheduleResponse> Schedules,
    IReadOnlyList<Guid> PrerequisiteCourseIds)
{
    public static CourseAdministrationResponse From(AdminCourseDto course) => new(
        course.Id,
        course.Code,
        course.Name,
        course.Description,
        course.Credits,
        course.Capacity,
        course.Semester,
        course.IsActive,
        course.ActiveEnrollmentCount,
        course.Schedules.Select(ScheduleResponse.From).ToList(),
        course.PrerequisiteCourseIds);
}

/// <summary>The HTTP view of a course meeting slot.</summary>
public sealed record ScheduleResponse(Guid Id, DayOfWeek DayOfWeek, string StartTime, string EndTime)
{
    public static ScheduleResponse From(CourseScheduleDto schedule) => new(
        schedule.Id, schedule.DayOfWeek, schedule.StartTime, schedule.EndTime);
}

/// <summary>The body of a create- or update-course request.</summary>
public sealed record SaveCourseRequest
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Credits { get; init; }
    public int Capacity { get; init; }
    public string Semester { get; init; } = string.Empty;
    public IReadOnlyList<ScheduleInput> Schedules { get; init; } = Array.Empty<ScheduleInput>();
    public IReadOnlyList<Guid> PrerequisiteCourseIds { get; init; } = Array.Empty<Guid>();
}

/// <summary>A weekly meeting slot supplied in a save-course request.</summary>
public sealed record ScheduleInput(DayOfWeek DayOfWeek, string StartTime, string EndTime);

/// <summary>The body of a status-change request.</summary>
public sealed record ChangeStudentStatusRequest(string Status);
