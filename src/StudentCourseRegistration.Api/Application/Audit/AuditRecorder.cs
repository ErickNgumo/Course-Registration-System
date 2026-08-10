using System.Text.Json;
using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Domain.Audit;

namespace StudentCourseRegistration.Api.Application.Audit;

/// <summary>Creates and persists audit log entries for administrative actions.</summary>
public sealed class AuditRecorder : IAuditRecorder
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private readonly IAuditLogRepository _auditRepository;

    public AuditRecorder(IAuditLogRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    /// <inheritdoc />
    public Task RecordAsync(
        Guid administratorId,
        string action,
        string entity,
        Guid entityId,
        object? previousState,
        object? newState,
        CancellationToken cancellationToken)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            AdministratorId = administratorId,
            Action = action,
            Entity = entity,
            EntityId = entityId,
            Timestamp = DateTimeOffset.UtcNow,
            OldValues = Serialize(previousState),
            NewValues = Serialize(newState)
        };
        return _auditRepository.AddAsync(auditLog, cancellationToken);
    }

    private static string? Serialize(object? state) =>
        state is null ? null : JsonSerializer.Serialize(state, SerializerOptions);
}
