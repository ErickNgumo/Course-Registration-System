using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentCourseRegistration.Api.Domain.Courses;

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Configurations;

public sealed class CoursePrerequisiteConfiguration : IEntityTypeConfiguration<CoursePrerequisite>
{
    public void Configure(EntityTypeBuilder<CoursePrerequisite> builder)
    {
        builder.ToTable("CoursePrerequisites", table =>
        {
            table.HasCheckConstraint("CK_CoursePrerequisites_Prerequisite_Differs", "[CourseId] <> [PrerequisiteCourseId]");
        });

        builder.HasKey(prerequisite => new { prerequisite.CourseId, prerequisite.PrerequisiteCourseId });

        builder.Property(prerequisite => prerequisite.CourseId).IsRequired();
        builder.Property(prerequisite => prerequisite.PrerequisiteCourseId).IsRequired();
        builder.Property(prerequisite => prerequisite.CreatedAt).IsRequired();

        builder.HasOne<Course>()
            .WithMany()
            .HasForeignKey(prerequisite => prerequisite.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Course>()
            .WithMany()
            .HasForeignKey(prerequisite => prerequisite.PrerequisiteCourseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
