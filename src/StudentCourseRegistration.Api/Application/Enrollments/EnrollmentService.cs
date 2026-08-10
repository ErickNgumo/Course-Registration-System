using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Application.Common.Exceptions;
using StudentCourseRegistration.Api.Domain.Courses;
using StudentCourseRegistration.Api.Domain.Enrollments;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Application.Enrollments;

/// <summary>Owns all student registration business rules and the course capacity policy.</summary>
public sealed class EnrollmentService : IEnrollmentService
{
    private readonly ICourseRepository _courses;
    private readonly IEnrollmentRepository _enrollments;
    private readonly IPrerequisiteRepository _prerequisites;
    private readonly ISchedulingRepository _schedules;
    private readonly IStudentRepository _students;
    private readonly IUnitOfWork _unitOfWork;
    private readonly EnrollmentOptions _options;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(
        ICourseRepository courses,
        IEnrollmentRepository enrollments,
        IPrerequisiteRepository prerequisites,
        ISchedulingRepository schedules,
        IStudentRepository students,
        IUnitOfWork unitOfWork,
        IOptions<EnrollmentOptions> options,
        ILogger<EnrollmentService> logger)
    {
        _courses = courses;
        _enrollments = enrollments;
        _prerequisites = prerequisites;
        _schedules = schedules;
        _students = students;
        _unitOfWork = unitOfWork;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<EnrollmentDto> RegisterAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken)
    {
        var student = await _students.FindByIdAsync(studentId, cancellationToken);
        if (student is null || student.Status != StudentStatus.Active)
        {
            throw new ForbiddenException("Only active students may register for courses.");
        }

        var course = await _courses.FindByIdAsync(courseId, cancellationToken)
            ?? throw new NotFoundException("course");

        if (!course.IsActive)
        {
            throw new UnprocessableEntityException("Cannot register for an inactive course.");
        }

        if (await _enrollments.FindActiveAsync(studentId, courseId, cancellationToken) is not null)
        {
            throw new ConflictException("The student is already enrolled in this course.");
        }

        await EnsurePrerequisitesMetAsync(studentId, courseId, cancellationToken);
        await EnsureNoTimetableConflictAsync(studentId, course, cancellationToken);
        await EnsureWithinCreditLimitAsync(studentId, course, cancellationToken);

        var registeredCount = await _enrollments.CountRegisteredAsync(courseId, cancellationToken);
        var isFull = registeredCount >= course.Capacity;
        var status = isFull ? EnrollmentStatus.Waitlisted : EnrollmentStatus.Registered;

        if (isFull && !_options.WaitlistEnabled)
        {
            throw new ConflictException("The course is full and waitlisting is disabled.");
        }

        var now = DateTimeOffset.UtcNow;
        var enrollment = new Enrollment
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            CourseId = courseId,
            Status = status,
            RegisteredAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };
        var persisted = await _enrollments.UpsertAsync(enrollment, cancellationToken);

        _logger.LogInformation(
            "Student {StudentId} registered for course {CourseId} with status {Status}.",
            studentId, courseId, status);

        return await MapEnrollmentAsync(persisted, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DropAsync(Guid studentId, Guid enrollmentId, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollments.FindByIdAsync(enrollmentId, studentId, cancellationToken)
            ?? throw new NotFoundException("enrollment");

        if (enrollment.Status is EnrollmentStatus.Dropped or EnrollmentStatus.Completed)
        {
            throw new ConflictException("This enrollment cannot be dropped.");
        }

        var wasRegistered = enrollment.Status == EnrollmentStatus.Registered;

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            enrollment.Status = EnrollmentStatus.Dropped;
            enrollment.DroppedAt = DateTimeOffset.UtcNow;
            enrollment.UpdatedAt = enrollment.DroppedAt.Value;
            await _enrollments.UpsertAsync(enrollment, cancellationToken);

            // Only a released seat frees capacity and warrants promoting a waiter.
            if (wasRegistered)
            {
                await TryPromoteFromWaitlistAsync(enrollment.CourseId, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        _logger.LogInformation(
            "Student {StudentId} dropped enrollment {EnrollmentId}.", studentId, enrollmentId);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EnrollmentDto>> GetEnrollmentsAsync(
        Guid studentId, CancellationToken cancellationToken)
    {
        var enrollments = await _enrollments.GetStudentEnrollmentsAsync(
            studentId, includeDropped: true, cancellationToken);
        return await MapEnrollmentsAsync(enrollments, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DashboardDto> GetDashboardAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var student = await _students.FindByIdAsync(studentId, cancellationToken)
            ?? throw new NotFoundException("student");

        var enrollments = await _enrollments.GetStudentEnrollmentsAsync(
            studentId, includeDropped: false, cancellationToken);
        var mapped = await MapEnrollmentsAsync(enrollments, cancellationToken);

        var registered = mapped.Where(e => e.Status == EnrollmentStatus.Registered).ToList();
        var currentSemester = registered.Count > 0 ? registered[0].Semester : string.Empty;
        var currentCredits = registered
            .Where(e => e.Semester == currentSemester)
            .Sum(e => e.Credits);

        return new DashboardDto(
            new DashboardStudentDto(student.Id, student.StudentNumber, student.FirstName, student.LastName, student.Email),
            currentCredits,
            _options.MaxSemesterCredits,
            registered,
            mapped.Where(e => e.Status == EnrollmentStatus.Waitlisted).ToList(),
            mapped.Where(e => e.Status == EnrollmentStatus.Completed).ToList());
    }

    private async Task EnsurePrerequisitesMetAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken)
    {
        var prerequisiteIds = await _prerequisites.GetPrerequisiteCourseIdsAsync(courseId, cancellationToken);
        if (prerequisiteIds.Count == 0)
        {
            return;
        }

        foreach (var prerequisiteId in prerequisiteIds)
        {
            var completed = await _enrollments.FindCompletedAsync(studentId, prerequisiteId, cancellationToken);
            if (completed is null)
            {
                throw new UnprocessableEntityException("A prerequisite course has not been completed.");
            }
        }
    }

    private async Task EnsureNoTimetableConflictAsync(Guid studentId, Course course, CancellationToken cancellationToken)
    {
        var active = await _enrollments.GetStudentEnrollmentsAsync(studentId, includeDropped: false, cancellationToken);
        var registeredCourseIds = active
            .Where(e => e.Status == EnrollmentStatus.Registered)
            .Select(e => e.CourseId)
            .Append(course.Id)
            .Distinct()
            .ToList();

        var schedulesByCourse = await _schedules.GetSchedulesForCoursesAsync(registeredCourseIds, cancellationToken);
        var candidateSlots = schedulesByCourse.GetValueOrDefault(course.Id) ?? new List<CourseSchedule>();

        foreach (var enrollment in active.Where(e => e.Status == EnrollmentStatus.Registered))
        {
            var existingSlots = schedulesByCourse.GetValueOrDefault(enrollment.CourseId) ?? new List<CourseSchedule>();
            if (HasConflict(existingSlots, candidateSlots))
            {
                throw new UnprocessableEntityException("Registration would create a timetable conflict.");
            }
        }
    }

    private static bool HasConflict(IReadOnlyList<CourseSchedule> first, IReadOnlyList<CourseSchedule> second)
    {
        if (first.Count == 0 || second.Count == 0)
        {
            return false;
        }

        foreach (var existing in first)
        {
            foreach (var candidate in second)
            {
                if (existing.DayOfWeek == candidate.DayOfWeek &&
                    existing.StartTime < candidate.EndTime &&
                    candidate.StartTime < existing.EndTime)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private async Task EnsureWithinCreditLimitAsync(Guid studentId, Course course, CancellationToken cancellationToken)
    {
        var currentCredits = await _enrollments.SumRegisteredCreditsAsync(studentId, course.Semester, cancellationToken);
        if (currentCredits + course.Credits > _options.MaxSemesterCredits)
        {
            throw new UnprocessableEntityException(
                $"Registering would exceed the maximum {_options.MaxSemesterCredits} credits for the semester.");
        }
    }

    private async Task TryPromoteFromWaitlistAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var waitlisted = await _enrollments.FindOldestWaitlistedAsync(courseId, cancellationToken);
        if (waitlisted is null)
        {
            return;
        }

        waitlisted.Status = EnrollmentStatus.Registered;
        waitlisted.UpdatedAt = DateTimeOffset.UtcNow;
        await _enrollments.UpsertAsync(waitlisted, cancellationToken);

        _logger.LogInformation(
            "Waitlisted student {StudentId} promoted in course {CourseId}.",
            waitlisted.StudentId, courseId);
    }

    private async Task<IReadOnlyList<EnrollmentDto>> MapEnrollmentsAsync(
        IReadOnlyList<Enrollment> enrollments, CancellationToken cancellationToken)
    {
        if (enrollments.Count == 0)
        {
            return Array.Empty<EnrollmentDto>();
        }

        var courses = await _courses.FindByIdsAsync(
            enrollments.Select(e => e.CourseId).Distinct().ToList(), cancellationToken);
        return enrollments
            .OrderBy(e => e.RegisteredAt)
            .Select(e => MapEnrollment(e, courses.GetValueOrDefault(e.CourseId)))
            .ToList();
    }

    private async Task<EnrollmentDto> MapEnrollmentAsync(
        Enrollment enrollment, CancellationToken cancellationToken)
    {
        var course = await _courses.FindByIdAsync(enrollment.CourseId, cancellationToken);
        return MapEnrollment(enrollment, course);
    }

    private static EnrollmentDto MapEnrollment(Enrollment enrollment, Course? course) => new(
        enrollment.Id,
        enrollment.CourseId,
        course?.Code ?? string.Empty,
        course?.Name ?? string.Empty,
        course?.Semester ?? string.Empty,
        course?.Credits ?? 0,
        enrollment.Status,
        enrollment.RegisteredAt,
        enrollment.DroppedAt,
        enrollment.FinalGrade);
}
