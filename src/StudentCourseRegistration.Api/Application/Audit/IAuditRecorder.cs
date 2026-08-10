namespace StudentCourseRegistration.Api.Application.Audit;

/// <summary>Creates and persists audit log entries for administrative actions.</summary>
public interface IAuditRecorder
{
    /// <summary>Records an administrative action against a domain entity.</summary>
    /// <param name="administratorId">The administrator performing the action.</param>
    /// <param name="action">A short label describing the action.</param>
    /// <param name="entity">The name of the affected entity type.</param>
    /// <param name="entityId">The identifier of the affected entity.</param>
    /// <param name="previousState">A snapshot of the entity before the action, or null.</param>
    /// <param name="newState">A snapshot of the entity after the action, or null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RecordAsync(
        Guid administratorId,
        string action,
        string entity,
        Guid entityId,
        object? previousState,
        object? newState,
        CancellationToken cancellationToken);
}
