using StudentCourseRegistration.Api.Domain.Courses;
using StudentCourseRegistration.Api.Domain.Enrollments;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Tests.Testing;

/// <summary>Factory helpers for building domain entities in unit tests.</summary>
internal static class Fixtures
{
    public static Course Course(
        Guid? id = null,
        string code = "CSC101",
        string name = "Introduction to Programming",
        int credits = 3,
        int capacity = 50,
        string semester = "Fall 2026",
        bool isActive = true)
    {
        var now = DateTimeOffset.UtcNow;
        return new Course
        {
            Id = id ?? Guid.NewGuid(),
            Code = code,
            Name = name,
            Description = $"{name} course",
            Credits = credits,
            Capacity = capacity,
            Semester = semester,
            IsActive = isActive,
            CreatedAt = now
        };
    }

    public static Enrollment Enrollment(
        Guid studentId,
        Guid courseId,
        EnrollmentStatus status = EnrollmentStatus.Registered,
        DateTimeOffset? registeredAt = null,
        string? finalGrade = null,
        Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        StudentId = studentId,
        CourseId = courseId,
        Status = status,
        RegisteredAt = registeredAt ?? DateTimeOffset.UtcNow,
        DroppedAt = status == EnrollmentStatus.Dropped ? DateTimeOffset.UtcNow : null,
        FinalGrade = finalGrade,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };

    public static CourseSchedule Slot(
        Guid courseId,
        DayOfWeek day,
        TimeOnly start,
        TimeOnly end,
        Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        CourseId = courseId,
        DayOfWeek = day,
        StartTime = start,
        EndTime = end,
        CreatedAt = DateTimeOffset.UtcNow
    };

    public static Student Student(Guid? id = null, string email = "john.doe@university.edu")
    {
        var now = DateTimeOffset.UtcNow;
        return new Student
        {
            Id = id ?? Guid.NewGuid(),
            StudentNumber = "STU-2026-001",
            FirstName = "John",
            LastName = "Doe",
            Email = email,
            Status = StudentStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
