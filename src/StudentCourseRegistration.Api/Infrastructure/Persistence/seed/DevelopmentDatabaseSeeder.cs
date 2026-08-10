using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentCourseRegistration.Api.Domain.Administrators;
using StudentCourseRegistration.Api.Domain.Courses;
using StudentCourseRegistration.Api.Domain.Enrollments;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Seed;

/// <summary>Creates deterministic development data without overwriting existing records.</summary>
public sealed class DevelopmentDatabaseSeeder
{
    private const string AdministratorEmail = "admin@university.edu";
    private const string StudentEmail = "john.doe@university.edu";
    private const string CompletedStudentEmail = "jane.smith@university.edu";
    private const string DevelopmentPassword = "Password123!";

    /// <summary>The <see cref="Course"/> the active student has already completed.</summary>
    public static readonly Guid CompletedCourseId = new("a1111111-0000-0000-0000-000000000001");

    /// <summary>The course requiring <see cref="CompletedCourseId"/> as a prerequisite.</summary>
    public static readonly Guid PrerequisiteTargetCourseId = new("a1111111-0000-0000-0000-000000000002");

    /// <summary>A course the active student is already registered for (timetable baseline).</summary>
    public static readonly Guid RegisteredCourseId = new("b2222222-0000-0000-0000-000000000001");

    /// <summary>A course whose schedule overlaps <see cref="RegisteredCourseId"/> (timetable conflict).</summary>
    public static readonly Guid ConflictingCourseId = new("b2222222-0000-0000-0000-000000000002");

    /// <summary>A course with no conflicts and no prerequisites (happy-path registration).</summary>
    public static readonly Guid OpenCourseId = new("b2222222-0000-0000-0000-000000000003");

    /// <summary>A low-capacity course used to exercise waitlist creation and promotion.</summary>
    public static readonly Guid WaitlistCourseId = new("c3333333-0000-0000-0000-000000000001");

    /// <summary>A course carrying 6 credits to exercise the semester credit limit.</summary>
    public static readonly Guid HighCreditCourseId = new("d4444444-0000-0000-0000-000000000001");

    private readonly RegistrationDbContext _context;
    private readonly IPasswordHasher<Administrator> _administratorPasswordHasher;
    private readonly IPasswordHasher<Student> _studentPasswordHasher;
    private readonly ILogger<DevelopmentDatabaseSeeder> _logger;

    public DevelopmentDatabaseSeeder(
        RegistrationDbContext context,
        IPasswordHasher<Administrator> administratorPasswordHasher,
        IPasswordHasher<Student> studentPasswordHasher,
        ILogger<DevelopmentDatabaseSeeder> logger)
    {
        _context = context;
        _administratorPasswordHasher = administratorPasswordHasher;
        _studentPasswordHasher = studentPasswordHasher;
        _logger = logger;
    }

    /// <summary>Creates development reference data that is missing from the database.</summary>
    public async Task SeedAsync()
    {
        var adminCreated = await SeedAdministratorAsync();
        var studentCreated = await SeedActiveStudentAsync();
        await SeedCompletedStudentAsync();
        var coursesCreated = await SeedCoursesAsync();
        var schedulesCreated = await SeedSchedulesAsync();
        var prerequisitesCreated = await SeedPrerequisitesAsync();
        var completionSeeded = await SeedCompletedEnrollmentAsync();
        var registrationSeeded = await SeedActiveRegistrationAsync();
        await SeedWaitlistFillersAsync();
        var auditsCreated = await SeedAuditLogsAsync();

        if (adminCreated || studentCreated || coursesCreated > 0 || schedulesCreated > 0 ||
            prerequisitesCreated > 0 || completionSeeded || registrationSeeded || auditsCreated > 0)
        {
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation(
            "Development database seeding completed. AdministratorCreated: {AdministratorCreated}, " +
            "StudentCreated: {StudentCreated}, CoursesCreated: {CoursesCreated}, SchedulesCreated: {SchedulesCreated}, " +
            "PrerequisitesCreated: {PrerequisitesCreated}.",
            adminCreated,
            studentCreated,
            coursesCreated,
            schedulesCreated,
            prerequisitesCreated);
    }

    private async Task<bool> SeedAdministratorAsync()
    {
        if (await _context.Administrators.AnyAsync(administrator => administrator.Email == AdministratorEmail))
        {
            return false;
        }

        var administrator = new Administrator
        {
            Id = Guid.NewGuid(),
            FirstName = "System",
            LastName = "Administrator",
            Email = AdministratorEmail,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        administrator.PasswordHash = _administratorPasswordHasher.HashPassword(administrator, DevelopmentPassword);
        _context.Administrators.Add(administrator);
        return true;
    }

    private async Task<bool> SeedActiveStudentAsync()
    {
        if (await _context.Students.AnyAsync(student => student.Email == StudentEmail))
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        var student = DevelopmentSeedData.NewStudent(StudentEmail, "STU-2026-001", "John", "Doe", now);
        student.PasswordHash = _studentPasswordHasher.HashPassword(student, DevelopmentPassword);
        _context.Students.Add(student);
        return true;
    }

    private async Task SeedCompletedStudentAsync()
    {
        if (await _context.Students.AnyAsync(student => student.Email == CompletedStudentEmail))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var student = DevelopmentSeedData.NewStudent(CompletedStudentEmail, "STU-2026-002", "Jane", "Smith", now);
        student.PasswordHash = _studentPasswordHasher.HashPassword(student, DevelopmentPassword);
        _context.Students.Add(student);
    }

    private async Task<int> SeedCoursesAsync()
    {
        var courses = DevelopmentSeedData.BuildCourseRoster(DateTimeOffset.UtcNow);
        var existingCodes = await _context.Courses
            .Where(course => courses.Select(c => c.Code).Contains(course.Code))
            .Select(course => course.Code)
            .ToListAsync();

        var missingCourses = courses
            .Where(course => !existingCodes.Contains(course.Code, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (missingCourses.Count > 0)
        {
            _context.Courses.AddRange(missingCourses);
        }

        return missingCourses.Count;
    }

    private async Task<int> SeedSchedulesAsync()
    {
        var slots = DevelopmentSeedData.BuildScheduleRoster(DateTimeOffset.UtcNow);
        var keySelector = (CourseSchedule slot) => new { slot.CourseId, slot.DayOfWeek, slot.StartTime, slot.EndTime };
        var existingKeys = await _context.CourseSchedules
            .Where(slot => slots.Select(s => s.CourseId).Contains(slot.CourseId))
            .Select(slot => new { slot.CourseId, slot.DayOfWeek, slot.StartTime, slot.EndTime })
            .ToListAsync();

        var missing = slots
            .Where(slot => !existingKeys.Contains(keySelector(slot)))
            .ToList();

        if (missing.Count > 0)
        {
            _context.CourseSchedules.AddRange(missing);
        }

        return missing.Count;
    }

    private async Task<int> SeedPrerequisitesAsync()
    {
        var chains = DevelopmentSeedData.BuildPrerequisiteChains();
        var requested = chains
            .Select(c => new CoursePrerequisiteKey(c.CourseId, c.PrerequisiteCourseId))
            .ToList();

        var existing = await _context.CoursePrerequisites
            .Where(prereq => requested.Select(c => c.CourseId).Contains(prereq.CourseId))
            .Select(prereq => new CoursePrerequisiteKey(prereq.CourseId, prereq.PrerequisiteCourseId))
            .ToListAsync();

        var missing = requested
            .Where(c => !existing.Contains(c))
            .Select(c => new CoursePrerequisite
            {
                CourseId = c.CourseId,
                PrerequisiteCourseId = c.PrerequisiteCourseId,
                CreatedAt = DateTimeOffset.UtcNow
            })
            .ToList();

        if (missing.Count > 0)
        {
            _context.CoursePrerequisites.AddRange(missing);
        }

        return missing.Count;
    }

    private sealed record CoursePrerequisiteKey(Guid CourseId, Guid PrerequisiteCourseId);

    private async Task<bool> SeedCompletedEnrollmentAsync()
    {
        var student = await _context.Students.SingleOrDefaultAsync(s => s.Email == StudentEmail);
        if (student is null ||
            await _context.Enrollments.AnyAsync(e =>
                e.StudentId == student.Id && e.CourseId == CompletedCourseId &&
                e.Status == EnrollmentStatus.Completed))
        {
            return false;
        }

        _context.Enrollments.Add(DevelopmentSeedData.CompletedEnrollment(student.Id));
        return true;
    }

    private async Task<bool> SeedActiveRegistrationAsync()
    {
        var student = await _context.Students.SingleOrDefaultAsync(s => s.Email == StudentEmail);
        if (student is null ||
            await _context.Enrollments.AnyAsync(e =>
                e.StudentId == student.Id && e.CourseId == RegisteredCourseId &&
                e.Status == EnrollmentStatus.Registered))
        {
            return false;
        }

        _context.Enrollments.Add(DevelopmentSeedData.RegisteredEnrollment(student.Id));
        return true;
    }

    private async Task SeedWaitlistFillersAsync()
    {
        // Fill the single seat of the waitlist course so the active student lands on the waitlist,
        // and queue one waiter to validate automatic promotion when that seat drops.
        var now = DateTimeOffset.UtcNow;
        var seatOwner = await GetOrCreateStudentAsync("waitlist.seat@university.edu", "STU-2026-003", "Seat", "Holder", now);
        var waiter = await GetOrCreateStudentAsync("waitlist.waiter@university.edu", "STU-2026-004", "Waiter", "One", now);

        if (!await _context.Enrollments.AnyAsync(e =>
                e.StudentId == seatOwner.Id && e.CourseId == WaitlistCourseId &&
                e.Status == EnrollmentStatus.Registered))
        {
            _context.Enrollments.Add(DevelopmentSeedData.SeatHolderEnrollment(seatOwner.Id));
        }

        if (!await _context.Enrollments.AnyAsync(e =>
                e.StudentId == waiter.Id && e.CourseId == WaitlistCourseId &&
                e.Status == EnrollmentStatus.Waitlisted))
        {
            _context.Enrollments.Add(DevelopmentSeedData.WaiterEnrollment(waiter.Id));
        }
    }

    private async Task<Student> GetOrCreateStudentAsync(
        string email, string studentNumber, string firstName, string lastName, DateTimeOffset now)
    {
        var existing = await _context.Students.SingleOrDefaultAsync(s => s.Email == email);
        if (existing is not null)
        {
            return existing;
        }

        var student = DevelopmentSeedData.NewStudent(email, studentNumber, firstName, lastName, now);
        student.PasswordHash = _studentPasswordHasher.HashPassword(student, DevelopmentPassword);
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        return student;
    }

    private async Task<int> SeedAuditLogsAsync()
    {
        if (await _context.AuditLogs.AnyAsync())
        {
            return 0;
        }

        var admin = await _context.Administrators.FirstOrDefaultAsync(a => a.Email == AdministratorEmail);
        if (admin is null)
        {
            return 0;
        }

        var now = DateTimeOffset.UtcNow;
        var auditEntries = new[]
        {
            CreateAuditLog(admin.Id, "StudentStatusChanged", "Student", Guid.NewGuid(), now),
            CreateAuditLog(admin.Id, "CourseCreated", "Course", Guid.NewGuid(), now),
            CreateAuditLog(admin.Id, "CourseUpdated", "Course", Guid.NewGuid(), now)
        };
        _context.AuditLogs.AddRange(auditEntries);
        return auditEntries.Length;
    }

    private static Domain.Audit.AuditLog CreateAuditLog(
        Guid administratorId, string action, string entity, Guid entityId, DateTimeOffset now) => new()
    {
        Id = Guid.NewGuid(),
        AdministratorId = administratorId,
        Action = action,
        Entity = entity,
        EntityId = entityId,
        Timestamp = now,
        OldValues = null,
        NewValues = null
    };
}
