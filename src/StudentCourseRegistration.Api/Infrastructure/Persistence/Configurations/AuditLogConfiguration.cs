using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentCourseRegistration.Api.Domain.Audit;

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(audit => audit.Id);

        builder.Property(audit => audit.AdministratorId).IsRequired();
        builder.Property(audit => audit.Action).HasMaxLength(50).IsRequired();
        builder.Property(audit => audit.Entity).HasMaxLength(50).IsRequired();
        builder.Property(audit => audit.EntityId).IsRequired();
        builder.Property(audit => audit.Timestamp).IsRequired();
        builder.Property(audit => audit.OldValues).HasColumnType("nvarchar(max)");
        builder.Property(audit => audit.NewValues).HasColumnType("nvarchar(max)");

        builder.HasIndex(audit => audit.Timestamp);
        builder.HasIndex(audit => new { audit.Entity, audit.EntityId });
        builder.HasIndex(audit => audit.AdministratorId);
    }
}
