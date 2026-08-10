using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentCourseRegistration.Api.Domain.Courses;

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Configurations;

public sealed class CourseScheduleConfiguration : IEntityTypeConfiguration<CourseSchedule>
{
    public void Configure(EntityTypeBuilder<CourseSchedule> builder)
    {
        builder.ToTable("CourseSchedules", table =>
        {
            table.HasCheckConstraint("CK_CourseSchedules_EndTime_AfterStartTime", "[EndTime] > [StartTime]");
        });

        builder.HasKey(schedule => schedule.Id);

        builder.Property(schedule => schedule.CourseId).IsRequired();
        builder.Property(schedule => schedule.DayOfWeek).HasConversion<int>().IsRequired();
        builder.Property(schedule => schedule.StartTime).HasColumnType("time").IsRequired();
        builder.Property(schedule => schedule.EndTime).HasColumnType("time").IsRequired();
        builder.Property(schedule => schedule.CreatedAt).IsRequired();

        builder.HasOne<Course>()
            .WithMany()
            .HasForeignKey(schedule => schedule.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // A course cannot have the same meeting slot defined twice.
        builder.HasIndex(schedule => new { schedule.CourseId, schedule.DayOfWeek, schedule.StartTime, schedule.EndTime })
            .IsUnique();
    }
}
