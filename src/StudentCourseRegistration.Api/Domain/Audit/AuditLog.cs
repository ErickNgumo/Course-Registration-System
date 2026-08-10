namespace StudentCourseRegistration.Api.Domain.Audit;

/// <summary>An immutable record of an administrative action performed against a domain entity.</summary>
public sealed class AuditLog
{
    /// <summary>The unique identifier of the audit record.</summary>
    public Guid Id { get; set; }

    /// <summary>The administrator who performed the action.</summary>
    public Guid AdministratorId { get; set; }

    /// <summary>A short label describing the performed action.</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>The name of the affected entity type.</summary>
    public string Entity { get; set; } = string.Empty;

    /// <summary>The identifier of the affected entity.</summary>
    public Guid EntityId { get; set; }

    /// <summary>The moment the action occurred.</summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>A JSON snapshot of the entity state before the action, if applicable.</summary>
    public string? OldValues { get; set; }

    /// <summary>A JSON snapshot of the entity state after the action, if applicable.</summary>
    public string? NewValues { get; set; }
}
