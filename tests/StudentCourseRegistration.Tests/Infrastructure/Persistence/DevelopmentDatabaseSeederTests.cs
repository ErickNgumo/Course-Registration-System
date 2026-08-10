using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using StudentCourseRegistration.Api.Domain.Administrators;
using StudentCourseRegistration.Api.Domain.Courses;
using StudentCourseRegistration.Api.Domain.Enrollments;
using StudentCourseRegistration.Api.Domain.Students;
using StudentCourseRegistration.Api.Infrastructure.Persistence;
using StudentCourseRegistration.Api.Infrastructure.Persistence.Seed;
using Xunit;

namespace StudentCourseRegistration.Tests.Infrastructure.Persistence;

public sealed class DevelopmentDatabaseSeederTests
{
    [Fact]
    public async Task SeedAsync_CreatesExpectedDataOnlyOnce()
    {
        await using var context = CreateContext();
        var seeder = new DevelopmentDatabaseSeeder(
            context,
            new PasswordHasher<Administrator>(),
            new PasswordHasher<Student>(),
            NullLogger<DevelopmentDatabaseSeeder>.Instance);

        await seeder.SeedAsync();
        await seeder.SeedAsync();

        Assert.Equal(1, await context.Administrators.CountAsync());
        Assert.Equal(4, await context.Students.CountAsync());
        Assert.Equal(15, await context.Courses.CountAsync());
        Assert.All(await context.Courses.ToListAsync(), course => Assert.True(course.IsActive));

        // Prerequisite chain and schedules are seeded exactly once.
        Assert.Equal(1, await context.CoursePrerequisites.CountAsync());
        Assert.True(await context.CourseSchedules.AnyAsync());

        // The active student has one completed and one registered enrollment.
        var student = await context.Students.SingleAsync(s => s.Email == "john.doe@university.edu");
        Assert.Equal(1, await context.Enrollments.CountAsync(e =>
            e.StudentId == student.Id && e.Status == EnrollmentStatus.Completed));
        Assert.Equal(1, await context.Enrollments.CountAsync(e =>
            e.StudentId == student.Id && e.Status == EnrollmentStatus.Registered));
    }

    [Fact]
    public async Task SeedAsync_HashesSeededPasswords()
    {
        await using var context = CreateContext();
        var studentHasher = new PasswordHasher<Student>();
        var administratorHasher = new PasswordHasher<Administrator>();
        var seeder = new DevelopmentDatabaseSeeder(
            context,
            administratorHasher,
            studentHasher,
            NullLogger<DevelopmentDatabaseSeeder>.Instance);

        await seeder.SeedAsync();

        var student = await context.Students.SingleAsync(s => s.Email == "john.doe@university.edu");
        var administrator = await context.Administrators.SingleAsync();

        Assert.NotEqual("Password123!", student.PasswordHash);
        Assert.NotEqual("Password123!", administrator.PasswordHash);
        Assert.NotEqual(PasswordVerificationResult.Failed, studentHasher.VerifyHashedPassword(student, student.PasswordHash, "Password123!"));
        Assert.NotEqual(PasswordVerificationResult.Failed, administratorHasher.VerifyHashedPassword(administrator, administrator.PasswordHash, "Password123!"));
    }

    [Fact]
    public async Task SeedAsync_FillsWaitlistCourse_WithRegisteredSeatAndWaitlistedStudent()
    {
        await using var context = CreateContext();
        var seeder = new DevelopmentDatabaseSeeder(
            context,
            new PasswordHasher<Administrator>(),
            new PasswordHasher<Student>(),
            NullLogger<DevelopmentDatabaseSeeder>.Instance);

        await seeder.SeedAsync();

        var seatHolder = await context.Students.SingleAsync(s => s.Email == "waitlist.seat@university.edu");
        var waiter = await context.Students.SingleAsync(s => s.Email == "waitlist.waiter@university.edu");

        Assert.Equal(1, await context.Enrollments.CountAsync(e =>
            e.StudentId == seatHolder.Id && e.CourseId == DevelopmentDatabaseSeeder.WaitlistCourseId &&
            e.Status == EnrollmentStatus.Registered));
        Assert.Equal(1, await context.Enrollments.CountAsync(e =>
            e.StudentId == waiter.Id && e.CourseId == DevelopmentDatabaseSeeder.WaitlistCourseId &&
            e.Status == EnrollmentStatus.Waitlisted));
    }

    private static RegistrationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RegistrationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new RegistrationDbContext(options);
    }
}
