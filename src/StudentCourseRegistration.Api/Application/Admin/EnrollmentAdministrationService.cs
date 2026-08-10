using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Application.Audit;
using StudentCourseRegistration.Api.Application.Common.Exceptions;
using StudentCourseRegistration.Api.Application.Common.Pagination;
using StudentCourseRegistration.Api.Domain.Courses;
using StudentCourseRegistration.Api.Domain.Enrollments;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Application.Admin;

/// <summary>Applies enrollment management business rules and records administrative audit entries.</summary>
public sealed class EnrollmentAdministrationService : IEnrollmentAdministrationService
{
    private readonly IEnrollmentAdministrationRepository _enrollments;
    private readonly ICourseRepository _courses;
    private readonly IStudentRepository _students;
    private readonly IAuditRecorder _auditRecorder;

    public EnrollmentAdministrationService(
        IEnrollmentAdministrationRepository enrollments,
        ICourseRepository courses,
        IStudentRepository students,
        IAuditRecorder auditRecorder)
    {
        _enrollments = enrollments;
        _courses = courses;
        _students = students;
        _auditRecorder = auditRecorder;
    }

    /// <inheritdoc />
    public async Task<PagedResult<AdminEnrollmentDto>> ListAsync(
        EnrollmentStatus? status,
        string? semester,
        Guid? courseId,
        Guid? studentId,
        string? sortBy,
        PageQuery page,
        CancellationToken cancellationToken)
    {
        var paged = await _enrollments.SearchAsync(status, semester, courseId, studentId, sortBy, page, cancellationToken);
        if (paged.Items.Count == 0)
        {
            return PagedResultFactory.Create(Array.Empty<AdminEnrollmentDto>(), paged.Page, paged.PageSize, paged.TotalItems);
        }

        var courseIds = paged.Items.Select(e => e.CourseId).Distinct().ToList();
        var studentIds = paged.Items.Select(e => e.StudentId).Distinct().ToList();
        var courses = await _courses.FindByIdsAsync(courseIds, cancellationToken);
        var students = await FindStudentsByIdsAsync(studentIds, cancellationToken);

        var dtos = paged.Items
            .Select(enrollment => MapEnrollment(
                enrollment,
                courses.GetValueOrDefault(enrollment.CourseId),
                students.GetValueOrDefault(enrollment.StudentId)))
            .ToList();

        return PagedResultFactory.Create(dtos, paged.Page, paged.PageSize, paged.TotalItems);
    }

    /// <inheritdoc />
    public async Task DropAsync(Guid administratorId, Guid enrollmentId, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollments.FindByIdAsync(enrollmentId, cancellationToken)
            ?? throw new NotFoundException("enrollment");

        if (enrollment.Status is EnrollmentStatus.Dropped)
        {
            throw new ConflictException("This enrollment is already dropped.");
        }

        if (enrollment.Status == EnrollmentStatus.Completed)
        {
            throw new ConflictException("Completed enrollments cannot be dropped.");
        }

        var wasRegistered = enrollment.Status == EnrollmentStatus.Registered;
        var previousStatus = enrollment.Status;

        enrollment.Status = EnrollmentStatus.Dropped;
        enrollment.DroppedAt = DateTimeOffset.UtcNow;
        enrollment.UpdatedAt = enrollment.DroppedAt.Value;
        await _enrollments.UpdateAsync(enrollment, cancellationToken);

        if (wasRegistered)
        {
            await TryPromoteFromWaitlistAsync(enrollment.CourseId, cancellationToken);
        }

        await _auditRecorder.RecordAsync(
            administratorId, "EnrollmentForceDropped", "Enrollment", enrollmentId, previousStatus.ToString(), "Dropped", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AdminEnrollmentDto> ApproveWaitlistPromotionAsync(
        Guid administratorId, Guid courseId, CancellationToken cancellationToken)
    {
        var promoted = await _enrollments.FindOldestWaitlistedAsync(courseId, cancellationToken)
            ?? throw new NotFoundException("waitlisted enrollment");

        var previousStatus = promoted.Status;
        promoted.Status = EnrollmentStatus.Registered;
        promoted.DroppedAt = null;
        promoted.UpdatedAt = DateTimeOffset.UtcNow;
        await _enrollments.UpdateAsync(promoted, cancellationToken);

        await _auditRecorder.RecordAsync(
            administratorId, "WaitlistPromotionApproved", "Enrollment", promoted.Id, previousStatus.ToString(), "Registered", cancellationToken);

        var course = await _courses.FindByIdAsync(promoted.CourseId, cancellationToken);
        var student = await _students.FindByIdAsync(promoted.StudentId, cancellationToken);
        return MapEnrollment(promoted, course, student);
    }

    /// <inheritdoc />
    public async Task<AdminEnrollmentDto> AssignGradeAsync(
        Guid administratorId, Guid enrollmentId, string? finalGrade, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollments.FindByIdAsync(enrollmentId, cancellationToken)
            ?? throw new NotFoundException("enrollment");

        if (!string.IsNullOrWhiteSpace(finalGrade) && finalGrade!.Trim().Length > 10)
        {
            throw new UnprocessableEntityException("Final grade must be at most 10 characters.");
        }

        var previousGrade = enrollment.FinalGrade;
        var previousStatus = enrollment.Status;

        enrollment.FinalGrade = string.IsNullOrWhiteSpace(finalGrade) ? null : finalGrade!.Trim();
        enrollment.Status = EnrollmentStatus.Completed;
        enrollment.UpdatedAt = DateTimeOffset.UtcNow;
        await _enrollments.UpdateAsync(enrollment, cancellationToken);

        await _auditRecorder.RecordAsync(
            administratorId, "GradeAssigned", "Enrollment", enrollmentId,
            new { previousStatus, previousGrade },
            new { status = enrollment.Status.ToString(), grade = enrollment.FinalGrade },
            cancellationToken);

        var course = await _courses.FindByIdAsync(enrollment.CourseId, cancellationToken);
        var student = await _students.FindByIdAsync(enrollment.StudentId, cancellationToken);
        return MapEnrollment(enrollment, course, student);
    }

    private async Task<IReadOnlyDictionary<Guid, Student>> FindStudentsByIdsAsync(
        IReadOnlyCollection<Guid> studentIds, CancellationToken cancellationToken)
    {
        var distinct = studentIds.Distinct().ToList();
        var students = new List<Student>();
        foreach (var id in distinct)
        {
            var student = await _students.FindByIdAsync(id, cancellationToken);
            if (student is not null)
            {
                students.Add(student);
            }
        }

        return students.ToDictionary(student => student.Id);
    }

    private async Task TryPromoteFromWaitlistAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var waiter = await _enrollments.FindOldestWaitlistedAsync(courseId, cancellationToken);
        if (waiter is null)
        {
            return;
        }

        waiter.Status = EnrollmentStatus.Registered;
        waiter.UpdatedAt = DateTimeOffset.UtcNow;
        await _enrollments.UpdateAsync(waiter, cancellationToken);
    }

    private static AdminEnrollmentDto MapEnrollment(Enrollment enrollment, Course? course, Student? student) => new(
        enrollment.Id,
        enrollment.StudentId,
        student?.Email ?? string.Empty,
        enrollment.CourseId,
        course?.Code ?? string.Empty,
        course?.Name ?? string.Empty,
        course?.Semester ?? string.Empty,
        enrollment.Status,
        enrollment.RegisteredAt,
        enrollment.DroppedAt,
        enrollment.FinalGrade);
}
