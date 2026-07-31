using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentCourseRegistration.Api.Domain.Administrators;

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Configurations;

public sealed class AdministratorConfiguration : IEntityTypeConfiguration<Administrator>
{
    public void Configure(EntityTypeBuilder<Administrator> builder)
    {
        builder.ToTable("Administrators");
        builder.HasKey(administrator => administrator.Id);
        builder.Property(administrator => administrator.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(administrator => administrator.LastName).HasMaxLength(100).IsRequired();
        builder.Property(administrator => administrator.Email).HasMaxLength(256).IsRequired();
        builder.Property(administrator => administrator.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(administrator => administrator.IsActive).IsRequired();
        builder.Property(administrator => administrator.CreatedAt).IsRequired();
        builder.HasIndex(administrator => administrator.Email).IsUnique();
    }
}
