using Microsoft.EntityFrameworkCore;
using StudentCourseRegistration.Api.Domain.Administrators;
using StudentCourseRegistration.Api.Domain.Audit;
using StudentCourseRegistration.Api.Domain.Courses;
using StudentCourseRegistration.Api.Domain.Enrollments;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Infrastructure.Persistence;

public sealed class RegistrationDbContext : DbContext
{
    public RegistrationDbContext(DbContextOptions<RegistrationDbContext> options) : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Administrator> Administrators => Set<Administrator>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<CourseSchedule> CourseSchedules => Set<CourseSchedule>();
    public DbSet<CoursePrerequisite> CoursePrerequisites => Set<CoursePrerequisite>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RegistrationDbContext).Assembly);
    }
}
