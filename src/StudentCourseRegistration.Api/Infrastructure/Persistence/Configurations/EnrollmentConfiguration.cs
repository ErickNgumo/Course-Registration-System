using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentCourseRegistration.Api.Domain.Courses;
using StudentCourseRegistration.Api.Domain.Enrollments;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Configurations;

public sealed class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments", table =>
        {
            table.HasCheckConstraint("CK_Enrollments_DroppedAt_AfterRegisteredAt", "[DroppedAt] IS NULL OR [DroppedAt] >= [RegisteredAt]");
        });

        builder.HasKey(enrollment => enrollment.Id);

        builder.Property(enrollment => enrollment.StudentId).IsRequired();
        builder.Property(enrollment => enrollment.CourseId).IsRequired();
        builder.Property(enrollment => enrollment.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(enrollment => enrollment.RegisteredAt).IsRequired();
        builder.Property(enrollment => enrollment.DroppedAt);
        builder.Property(enrollment => enrollment.FinalGrade).HasMaxLength(10);
        builder.Property(enrollment => enrollment.CreatedAt).IsRequired();
        builder.Property(enrollment => enrollment.UpdatedAt).IsRequired();

        builder.HasIndex(enrollment => new { enrollment.StudentId, enrollment.CourseId });

        // A student may hold at most one active enrollment (Registered or Waitlisted) per course.
        builder.HasIndex("StudentId", "CourseId", "Status")
            .HasFilter("[Status] IN ('Registered', 'Waitlisted')")
            .IsUnique();

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(enrollment => enrollment.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Course>()
            .WithMany()
            .HasForeignKey(enrollment => enrollment.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
