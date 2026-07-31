using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentCourseRegistration.Api.Domain.Courses;

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Configurations;

public sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses", table =>
        {
            table.HasCheckConstraint("CK_Courses_Credits_Positive", "[Credits] > 0");
            table.HasCheckConstraint("CK_Courses_Capacity_NonNegative", "[Capacity] >= 0");
        });

        builder.HasKey(course => course.Id);
        builder.Property(course => course.Code).HasMaxLength(20).IsRequired();
        builder.Property(course => course.Name).HasMaxLength(200).IsRequired();
        builder.Property(course => course.Description).HasMaxLength(2000);
        builder.Property(course => course.Credits).IsRequired();
        builder.Property(course => course.Capacity).IsRequired();
        builder.Property(course => course.Semester).HasMaxLength(50).IsRequired();
        builder.Property(course => course.IsActive).IsRequired();
        builder.Property(course => course.CreatedAt).IsRequired();
        builder.HasIndex(course => course.Code).IsUnique();
        builder.HasIndex(course => new { course.IsActive, course.Semester });
    }
}
