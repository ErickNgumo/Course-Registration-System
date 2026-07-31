using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");
        builder.HasKey(student => student.Id);
        builder.Property(student => student.StudentNumber).HasMaxLength(30).IsRequired();
        builder.Property(student => student.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(student => student.LastName).HasMaxLength(100).IsRequired();
        builder.Property(student => student.Email).HasMaxLength(256).IsRequired();
        builder.Property(student => student.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(student => student.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(student => student.CreatedAt).IsRequired();
        builder.Property(student => student.UpdatedAt).IsRequired();
        builder.HasIndex(student => student.StudentNumber).IsUnique();
        builder.HasIndex(student => student.Email).IsUnique();
    }
}
