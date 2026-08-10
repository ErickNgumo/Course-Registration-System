using StudentCourseRegistration.Api.Domain.Courses;
using StudentCourseRegistration.Api.Domain.Enrollments;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Seed;

/// <summary>Deterministic development seed rosters for courses, schedules, and prerequisites.</summary>
internal static class DevelopmentSeedData
{
    /// <summary>Builds the fifteen development courses.</summary>
    public static List<Course> BuildCourseRoster(DateTimeOffset now)
    {
        var fall = "Fall 2026";
        var spring = "Spring 2027";

        return new List<Course>
        {
            Course(DevelopmentDatabaseSeeder.CompletedCourseId, "CSC101", "Introduction to Programming", 3, 50, fall, now),
            Course(DevelopmentDatabaseSeeder.PrerequisiteTargetCourseId, "CSC102", "Object Oriented Programming", 3, 50, fall, now),
            Course(DevelopmentDatabaseSeeder.RegisteredCourseId, "CSC201", "Data Structures", 3, 40, fall, now),
            Course(DevelopmentDatabaseSeeder.ConflictingCourseId, "CSC202", "Algorithms", 3, 40, fall, now),
            Course(DevelopmentDatabaseSeeder.OpenCourseId, "CSC203", "Database Systems", 3, 40, fall, now),
            Course(DevelopmentDatabaseSeeder.WaitlistCourseId, "CSC204", "Operating Systems", 3, 1, fall, now),
            Course(DevelopmentDatabaseSeeder.HighCreditCourseId, "CSC205", "Computer Networks", 6, 30, fall, now),
            Course(Guid.NewGuid(), "CSC301", "Software Engineering", 3, 30, spring, now),
            Course(Guid.NewGuid(), "CSC302", "Distributed Systems", 3, 25, spring, now),
            Course(Guid.NewGuid(), "CSC303", "Artificial Intelligence", 3, 25, spring, now),
            Course(Guid.NewGuid(), "CSC304", "Machine Learning", 3, 25, spring, now),
            Course(Guid.NewGuid(), "CSC305", "Compilers", 3, 25, spring, now),
            Course(Guid.NewGuid(), "CSC306", "Computer Graphics", 3, 25, spring, now),
            Course(Guid.NewGuid(), "CSC307", "Theory of Computation", 3, 25, spring, now),
            Course(Guid.NewGuid(), "CSC308", "Cybersecurity", 3, 25, spring, now)
        };
    }

    /// <summary>Builds the weekly meeting schedules that support the test scenarios.</summary>
    public static List<CourseSchedule> BuildScheduleRoster(DateTimeOffset now)
    {
        var morning = new TimeOnly(9, 0);
        var morningEnd = new TimeOnly(10, 30);
        var lateMorning = new TimeOnly(9, 15);
        var lateMorningEnd = new TimeOnly(10, 45);
        var afternoon = new TimeOnly(13, 0);
        var afternoonEnd = new TimeOnly(14, 30);
        var evening = new TimeOnly(18, 0);
        var eveningEnd = new TimeOnly(19, 30);

        return new List<CourseSchedule>
        {
            // RegisteredCourseId and ConflictingCourseId overlap on Monday mornings.
            Slot(DevelopmentDatabaseSeeder.RegisteredCourseId, DayOfWeek.Monday, morning, morningEnd, now),
            Slot(DevelopmentDatabaseSeeder.ConflictingCourseId, DayOfWeek.Monday, lateMorning, lateMorningEnd, now),
            // OpenCourseId sits safely in the afternoon so it never conflicts.
            Slot(DevelopmentDatabaseSeeder.OpenCourseId, DayOfWeek.Tuesday, afternoon, afternoonEnd, now),
            Slot(DevelopmentDatabaseSeeder.CompletedCourseId, DayOfWeek.Wednesday, morning, morningEnd, now),
            Slot(DevelopmentDatabaseSeeder.PrerequisiteTargetCourseId, DayOfWeek.Wednesday, afternoon, afternoonEnd, now),
            Slot(DevelopmentDatabaseSeeder.WaitlistCourseId, DayOfWeek.Thursday, evening, eveningEnd, now),
            Slot(DevelopmentDatabaseSeeder.HighCreditCourseId, DayOfWeek.Friday, morning, morningEnd, now)
        };
    }

    /// <summary>Returns the prerequisite relationships that must hold between seeded courses.</summary>
    public static IReadOnlyList<(Guid CourseId, Guid PrerequisiteCourseId)> BuildPrerequisiteChains() => new[]
    {
        (DevelopmentDatabaseSeeder.PrerequisiteTargetCourseId, DevelopmentDatabaseSeeder.CompletedCourseId)
    };

    /// <summary>Builds a completed enrollment referencing the active student.</summary>
    public static Enrollment CompletedEnrollment(Guid studentId) =>
        BuildEnrollment(studentId, DevelopmentDatabaseSeeder.CompletedCourseId, EnrollmentStatus.Completed, DateTimeOffset.UtcNow.AddDays(-120), "A");

    /// <summary>Builds an active registered enrollment referencing the active student.</summary>
    public static Enrollment RegisteredEnrollment(Guid studentId)
    {
        var now = DateTimeOffset.UtcNow;
        return BuildEnrollment(studentId, DevelopmentDatabaseSeeder.RegisteredCourseId, EnrollmentStatus.Registered, now, null);
    }

    /// <summary>Builds a registered waitlist-seat-holder enrollment whose drop frees a seat.</summary>
    public static Enrollment SeatHolderEnrollment(Guid studentId)
    {
        var now = DateTimeOffset.UtcNow;
        return BuildEnrollment(studentId, DevelopmentDatabaseSeeder.WaitlistCourseId, EnrollmentStatus.Registered, now, null);
    }

    /// <summary>Builds the waitlisted enrollment that should be promoted when the seat drops.</summary>
    public static Enrollment WaiterEnrollment(Guid studentId)
    {
        var waitlistedAt = DateTimeOffset.UtcNow.AddSeconds(1);
        return BuildEnrollment(studentId, DevelopmentDatabaseSeeder.WaitlistCourseId, EnrollmentStatus.Waitlisted, waitlistedAt, null);
    }

    /// <summary>Builds an unhashed active student record; the caller assigns the password hash.</summary>
    public static Student NewStudent(string email, string studentNumber, string firstName, string lastName, DateTimeOffset now) => new()
    {
        Id = Guid.NewGuid(),
        Email = email,
        StudentNumber = studentNumber,
        FirstName = firstName,
        LastName = lastName,
        Status = StudentStatus.Active,
        CreatedAt = now,
        UpdatedAt = now
    };

    private static Enrollment BuildEnrollment(Guid studentId, Guid courseId, EnrollmentStatus status, DateTimeOffset registeredAt, string? finalGrade)
    {
        var droppedAt = status == EnrollmentStatus.Dropped ? DateTimeOffset.UtcNow : (DateTimeOffset?)null;
        return new Enrollment
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            CourseId = courseId,
            Status = status,
            RegisteredAt = registeredAt,
            DroppedAt = droppedAt,
            FinalGrade = finalGrade,
            CreatedAt = registeredAt,
            UpdatedAt = registeredAt
        };
    }

    private static Course Course(Guid id, string code, string name, int credits, int capacity, string semester, DateTimeOffset now) => new()
    {
        Id = id,
        Code = code,
        Name = name,
        Description = $"{name} course",
        Credits = credits,
        Capacity = capacity,
        Semester = semester,
        IsActive = true,
        CreatedAt = now
    };

    private static CourseSchedule Slot(Guid courseId, DayOfWeek day, TimeOnly start, TimeOnly end, DateTimeOffset now) => new()
    {
        Id = Guid.NewGuid(),
        CourseId = courseId,
        DayOfWeek = day,
        StartTime = start,
        EndTime = end,
        CreatedAt = now
    };
}
