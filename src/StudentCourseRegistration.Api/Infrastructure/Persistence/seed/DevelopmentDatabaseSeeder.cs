using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentCourseRegistration.Api.Domain.Administrators;
using StudentCourseRegistration.Api.Domain.Courses;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Seed;

/// <summary>Creates deterministic development data without overwriting existing records.</summary>
public sealed class DevelopmentDatabaseSeeder
{
    private const string AdministratorEmail = "admin@university.edu";
    private const string StudentEmail = "john.doe@university.edu";
    private const string DevelopmentPassword = "Password123!";
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
        var administratorCreated = await SeedAdministratorAsync();
        var studentCreated = await SeedStudentAsync();
        var coursesCreated = await SeedCoursesAsync();

        if (administratorCreated || studentCreated || coursesCreated > 0)
        {
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation(
            "Development database seeding completed. AdministratorCreated: {AdministratorCreated}, StudentCreated: {StudentCreated}, CoursesCreated: {CoursesCreated}.",
            administratorCreated,
            studentCreated,
            coursesCreated);
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

    private async Task<bool> SeedStudentAsync()
    {
        if (await _context.Students.AnyAsync(student => student.Email == StudentEmail))
        {
            return false;
        }

        var student = new Student
        {
            Id = Guid.NewGuid(),
            StudentNumber = "STU-2026-001",
            FirstName = "John",
            LastName = "Doe",
            Email = StudentEmail,
            Status = StudentStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        student.PasswordHash = _studentPasswordHasher.HashPassword(student, DevelopmentPassword);
        _context.Students.Add(student);
        return true;
    }

    private async Task<int> SeedCoursesAsync()
    {
        var courses = new[]
        {
            CreateCourse("CSC101", "Introduction to Programming", 3, 50, "Fall 2026"),
            CreateCourse("CSC102", "Object Oriented Programming", 3, 50, "Fall 2026"),
            CreateCourse("CSC201", "Data Structures", 3, 40, "Fall 2026"),
            CreateCourse("CSC202", "Algorithms", 3, 40, "Fall 2026"),
            CreateCourse("CSC203", "Database Systems", 3, 40, "Fall 2026"),
            CreateCourse("CSC204", "Operating Systems", 3, 35, "Fall 2026"),
            CreateCourse("CSC205", "Computer Networks", 3, 35, "Fall 2026"),
            CreateCourse("CSC301", "Software Engineering", 3, 30, "Spring 2027"),
            CreateCourse("CSC302", "Distributed Systems", 3, 25, "Spring 2027"),
            CreateCourse("CSC303", "Artificial Intelligence", 3, 25, "Spring 2027")
        };
        var courseCodes = courses.Select(course => course.Code).ToList();
        var existingCodes = await _context.Courses
            .Where(course => courseCodes.Contains(course.Code))
            .Select(course => course.Code)
            .ToListAsync();
        var missingCourses = courses
            .Where(course => !existingCodes.Contains(course.Code, StringComparer.OrdinalIgnoreCase))
            .ToList();

        _context.Courses.AddRange(missingCourses);
        return missingCourses.Count;
    }

    private static Course CreateCourse(string code, string name, int credits, int capacity, string semester) => new()
    {
        Id = Guid.NewGuid(),
        Code = code,
        Name = name,
        Description = $"{name} course",
        Credits = credits,
        Capacity = capacity,
        Semester = semester,
        IsActive = true,
        CreatedAt = DateTimeOffset.UtcNow
    };
}
