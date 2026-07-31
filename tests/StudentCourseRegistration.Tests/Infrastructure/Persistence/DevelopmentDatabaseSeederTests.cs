using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using StudentCourseRegistration.Api.Domain.Administrators;
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
        Assert.Equal(1, await context.Students.CountAsync());
        Assert.Equal(10, await context.Courses.CountAsync());
        Assert.All(await context.Courses.ToListAsync(), course => Assert.True(course.IsActive));
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

        var student = await context.Students.SingleAsync();
        var administrator = await context.Administrators.SingleAsync();

        Assert.NotEqual("Password123!", student.PasswordHash);
        Assert.NotEqual("Password123!", administrator.PasswordHash);
        Assert.NotEqual(PasswordVerificationResult.Failed, studentHasher.VerifyHashedPassword(student, student.PasswordHash, "Password123!"));
        Assert.NotEqual(PasswordVerificationResult.Failed, administratorHasher.VerifyHashedPassword(administrator, administrator.PasswordHash, "Password123!"));
    }

    private static RegistrationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RegistrationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new RegistrationDbContext(options);
    }
}
