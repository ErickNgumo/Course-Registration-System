using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Application.Audit;
using StudentCourseRegistration.Api.Application.Common.Exceptions;
using StudentCourseRegistration.Api.Application.Common.Pagination;
using StudentCourseRegistration.Api.Domain.Courses;
using StudentCourseRegistration.Api.Domain.Enrollments;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Application.Admin;

/// <summary>Applies student management business rules and records administrative audit entries.</summary>
public sealed class StudentAdministrationService : IStudentAdministrationService
{
    private readonly IStudentAdministrationRepository _students;
    private readonly IEnrollmentRepository _enrollments;
    private readonly ICourseRepository _courses;
    private readonly IAuditRecorder _auditRecorder;

    public StudentAdministrationService(
        IStudentAdministrationRepository students,
        IEnrollmentRepository enrollments,
        ICourseRepository courses,
        IAuditRecorder auditRecorder)
    {
        _students = students;
        _enrollments = enrollments;
        _courses = courses;
        _auditRecorder = auditRecorder;
    }

    /// <inheritdoc />
    public async Task<PagedResult<AdminStudentDto>> ListAsync(
        StudentStatus? status, string? search, string? sortBy, PageQuery page, CancellationToken cancellationToken)
    {
        var paged = await _students.SearchAsync(status, search, sortBy, page, cancellationToken);
        var dtos = paged.Items.Select(MapStudent).ToList();
        return PagedResultFactory.Create(dtos, paged.Page, paged.PageSize, paged.TotalItems);
    }

    /// <inheritdoc />
    public async Task<StudentProfileDto> GetAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var student = await _students.FindByIdAsync(studentId, cancellationToken)
            ?? throw new NotFoundException("student");

        var enrollments = await _enrollments.GetStudentEnrollmentsAsync(studentId, includeDropped: true, cancellationToken);
        var mapped = await MapEnrollmentsAsync(enrollments, cancellationToken);

        return new StudentProfileDto(
            MapStudent(student),
            mapped.Where(e => e.Status == EnrollmentStatus.Registered).ToList(),
            mapped.Where(e => e.Status == EnrollmentStatus.Completed).ToList(),
            mapped.Where(e => e.Status == EnrollmentStatus.Waitlisted).ToList(),
            mapped.ToList());
    }

    /// <inheritdoc />
    public async Task<AdminStudentDto> ChangeStatusAsync(
        Guid administratorId, Guid studentId, StudentStatus status, CancellationToken cancellationToken)
    {
        var student = await _students.FindByIdAsync(studentId, cancellationToken)
            ?? throw new NotFoundException("student");

        if (student.Status == status)
        {
            return MapStudent(student);
        }

        var previousStatus = student.Status;
        student.Status = status;
        student.UpdatedAt = DateTimeOffset.UtcNow;
        await _students.UpdateStatusAsync(student, cancellationToken);

        await _auditRecorder.RecordAsync(
            administratorId, "StudentStatusChanged", "Student", studentId, previousStatus.ToString(), status.ToString(), cancellationToken);

        return MapStudent(student);
    }

    private static AdminStudentDto MapStudent(Student student) => new(
        student.Id,
        student.StudentNumber,
        student.FirstName,
        student.LastName,
        student.Email,
        student.Status);

    private async Task<IReadOnlyList<AdminEnrollmentDto>> MapEnrollmentsAsync(
        IReadOnlyList<Enrollment> enrollments, CancellationToken cancellationToken)
    {
        if (enrollments.Count == 0)
        {
            return Array.Empty<AdminEnrollmentDto>();
        }

        var courses = await _courses.FindByIdsAsync(
            enrollments.Select(e => e.CourseId).Distinct().ToList(), cancellationToken);
        return enrollments
            .OrderBy(e => e.RegisteredAt)
            .Select(e => MapEnrollment(e, courses.GetValueOrDefault(e.CourseId)))
            .ToList();
    }

    private static AdminEnrollmentDto MapEnrollment(Enrollment enrollment, Course? course) => new(
        enrollment.Id,
        enrollment.StudentId,
        StudentEmail: string.Empty,
        enrollment.CourseId,
        course?.Code ?? string.Empty,
        course?.Name ?? string.Empty,
        course?.Semester ?? string.Empty,
        enrollment.Status,
        enrollment.RegisteredAt,
        enrollment.DroppedAt,
        enrollment.FinalGrade);
}
