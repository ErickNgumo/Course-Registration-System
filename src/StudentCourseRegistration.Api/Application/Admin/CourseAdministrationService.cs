using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Application.Audit;
using StudentCourseRegistration.Api.Application.Common.Exceptions;
using StudentCourseRegistration.Api.Application.Common.Pagination;
using StudentCourseRegistration.Api.Domain.Courses;
using StudentCourseRegistration.Api.Domain.Enrollments;

namespace StudentCourseRegistration.Api.Application.Admin;

/// <summary>Applies course management business rules and records administrative audit entries.</summary>
public sealed class CourseAdministrationService : ICourseAdministrationService
{
    private readonly ICourseAdministrationRepository _courses;
    private readonly IAuditRecorder _auditRecorder;

    public CourseAdministrationService(ICourseAdministrationRepository courses, IAuditRecorder auditRecorder)
    {
        _courses = courses;
        _auditRecorder = auditRecorder;
    }

    /// <inheritdoc />
    public async Task<PagedResult<AdminCourseDto>> ListAsync(
        string? search, string? sortBy, PageQuery page, CancellationToken cancellationToken)
    {
        var paged = await _courses.SearchAsync(search, sortBy, page, cancellationToken);
        var dtos = await Task.WhenAll(paged.Items.Select(course => MapAsync(course, cancellationToken)));
        return PagedResultFactory.Create(dtos, paged.Page, paged.PageSize, paged.TotalItems);
    }

    /// <inheritdoc />
    public async Task<AdminCourseDto> GetAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var course = await _courses.FindByIdAsync(courseId, cancellationToken)
            ?? throw new NotFoundException("course");
        return await MapAsync(course, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AdminCourseDto> CreateAsync(
        Guid administratorId, CreateCourseCommand command, CancellationToken cancellationToken)
    {
        EnsureValidCourseFields(command.Code, command.Name, command.Credits, command.Capacity, command.Semester);
        EnsureValidSchedule(command.Schedules);
        await EnsureCodeIsUniqueAsync(command.Code, null, cancellationToken);
        EnsureNoSelfPrerequisite(command.PrerequisiteCourseIds, null);

        var course = new Course
        {
            Id = Guid.NewGuid(),
            Code = command.Code.Trim(),
            Name = command.Name.Trim(),
            Description = command.Description,
            Credits = command.Credits,
            Capacity = command.Capacity,
            Semester = command.Semester.Trim(),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var persisted = await _courses.AddAsync(course, cancellationToken);
        await _courses.ReplaceScheduleAsync(persisted.Id, BuildSchedule(persisted.Id, command.Schedules), cancellationToken);
        await _courses.ReplacePrerequisitesAsync(persisted.Id, command.PrerequisiteCourseIds, cancellationToken);

        await _auditRecorder.RecordAsync(administratorId, "CourseCreated", "Course", persisted.Id, null, persisted, cancellationToken);
        return await MapAsync(persisted, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AdminCourseDto> UpdateAsync(
        Guid administratorId, Guid courseId, UpdateCourseCommand command, CancellationToken cancellationToken)
    {
        var course = await _courses.FindByIdAsync(courseId, cancellationToken)
            ?? throw new NotFoundException("course");
        var previousSnapshot = await MapAsync(course, cancellationToken);

        EnsureValidCourseFields(command.Code, command.Name, command.Credits, command.Capacity, command.Semester);
        EnsureValidSchedule(command.Schedules);
        await EnsureCodeIsUniqueAsync(command.Code, courseId, cancellationToken);
        EnsureNoSelfPrerequisite(command.PrerequisiteCourseIds, courseId);

        var activeEnrollments = await _courses.CountActiveEnrollmentsAsync(courseId, cancellationToken);
        if (command.Capacity < activeEnrollments)
        {
            throw new UnprocessableEntityException(
                $"Capacity cannot be reduced below the {activeEnrollments} active enrollments.");
        }

        course.Code = command.Code.Trim();
        course.Name = command.Name.Trim();
        course.Description = command.Description;
        course.Credits = command.Credits;
        course.Capacity = command.Capacity;
        course.Semester = command.Semester.Trim();
        var updated = await _courses.UpdateAsync(course, cancellationToken);
        await _courses.ReplaceScheduleAsync(courseId, BuildSchedule(courseId, command.Schedules), cancellationToken);
        await _courses.ReplacePrerequisitesAsync(courseId, command.PrerequisiteCourseIds, cancellationToken);

        var newState = await MapAsync(updated, cancellationToken);
        await _auditRecorder.RecordAsync(administratorId, "CourseUpdated", "Course", courseId, previousSnapshot, newState, cancellationToken);
        return newState;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid administratorId, Guid courseId, CancellationToken cancellationToken)
    {
        var course = await _courses.FindByIdAsync(courseId, cancellationToken)
            ?? throw new NotFoundException("course");

        var activeEnrollments = await _courses.CountActiveEnrollmentsAsync(courseId, cancellationToken);
        if (activeEnrollments > 0)
        {
            throw new ConflictException(
                $"Cannot delete a course with {activeEnrollments} active enrollment(s).");
        }

        await _courses.DeleteAsync(courseId, cancellationToken);
        await _auditRecorder.RecordAsync(administratorId, "CourseDeleted", "Course", courseId, course, null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AdminCourseDto> ActivateAsync(Guid administratorId, Guid courseId, CancellationToken cancellationToken)
    {
        var course = await _courses.FindByIdAsync(courseId, cancellationToken)
            ?? throw new NotFoundException("course");
        if (course.IsActive)
        {
            return await MapAsync(course, cancellationToken);
        }

        course.IsActive = true;
        var updated = await _courses.UpdateAsync(course, cancellationToken);
        await _auditRecorder.RecordAsync(administratorId, "CourseActivated", "Course", courseId, false, true, cancellationToken);
        return await MapAsync(updated, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AdminCourseDto> DeactivateAsync(Guid administratorId, Guid courseId, CancellationToken cancellationToken)
    {
        var course = await _courses.FindByIdAsync(courseId, cancellationToken)
            ?? throw new NotFoundException("course");
        if (!course.IsActive)
        {
            return await MapAsync(course, cancellationToken);
        }

        course.IsActive = false;
        var updated = await _courses.UpdateAsync(course, cancellationToken);
        await _auditRecorder.RecordAsync(administratorId, "CourseDeactivated", "Course", courseId, true, false, cancellationToken);
        return await MapAsync(updated, cancellationToken);
    }

    private static void EnsureValidCourseFields(string code, string name, int credits, int capacity, string semester)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new UnprocessableEntityException("Course code is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new UnprocessableEntityException("Course name is required.");
        }

        if (credits <= 0)
        {
            throw new UnprocessableEntityException("Credits must be positive.");
        }

        if (capacity < 0)
        {
            throw new UnprocessableEntityException("Capacity must be non-negative.");
        }

        if (string.IsNullOrWhiteSpace(semester))
        {
            throw new UnprocessableEntityException("Semester is required.");
        }
    }

    private static void EnsureValidSchedule(IReadOnlyList<CourseScheduleInput> schedules)
    {
        foreach (var slot in schedules)
        {
            if (!TimeOnly.TryParse(slot.StartTime, out var start) || !TimeOnly.TryParse(slot.EndTime, out var end))
            {
                throw new UnprocessableEntityException("Schedule times must be valid 24-hour times.");
            }

            if (end <= start)
            {
                throw new UnprocessableEntityException("Schedule end time must be after start time.");
            }
        }
    }

    private static void EnsureNoSelfPrerequisite(IReadOnlyList<Guid> prerequisiteIds, Guid? courseId)
    {
        if (courseId is { } id && prerequisiteIds.Contains(id))
        {
            throw new UnprocessableEntityException("A course cannot be a prerequisite of itself.");
        }
    }

    private async Task EnsureCodeIsUniqueAsync(string code, Guid? excludingCourseId, CancellationToken cancellationToken)
    {
        if (await _courses.CodeExistsAsync(code.Trim(), excludingCourseId, cancellationToken))
        {
            throw new ConflictException("A course with this code already exists.");
        }
    }

    private static IReadOnlyList<CourseSchedule> BuildSchedule(Guid courseId, IReadOnlyList<CourseScheduleInput> inputs)
    {
        var now = DateTimeOffset.UtcNow;
        return inputs
            .Select(input => new CourseSchedule
            {
                Id = Guid.NewGuid(),
                CourseId = courseId,
                DayOfWeek = input.DayOfWeek,
                StartTime = TimeOnly.Parse(input.StartTime),
                EndTime = TimeOnly.Parse(input.EndTime),
                CreatedAt = now
            })
            .ToList();
    }

    private async Task<AdminCourseDto> MapAsync(Course course, CancellationToken cancellationToken)
    {
        var schedule = await _courses.GetScheduleAsync(course.Id, cancellationToken);
        var prerequisiteIds = await _courses.GetPrerequisiteCourseIdsAsync(course.Id, cancellationToken);
        var activeEnrollments = await _courses.CountActiveEnrollmentsAsync(course.Id, cancellationToken);

        return new AdminCourseDto(
            course.Id,
            course.Code,
            course.Name,
            course.Description,
            course.Credits,
            course.Capacity,
            course.Semester,
            course.IsActive,
            activeEnrollments,
            schedule.Select(MapSchedule).ToList(),
            prerequisiteIds.ToList());
    }

    private static CourseScheduleDto MapSchedule(CourseSchedule slot) => new(
        slot.Id,
        slot.DayOfWeek,
        slot.StartTime.ToString("HH:mm"),
        slot.EndTime.ToString("HH:mm"));
}
