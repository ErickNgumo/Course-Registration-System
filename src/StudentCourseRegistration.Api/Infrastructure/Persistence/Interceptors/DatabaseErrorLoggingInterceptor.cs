using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Interceptors;

/// <summary>Logs database command failures without recording command text or parameter values.</summary>
public sealed class DatabaseErrorLoggingInterceptor : DbCommandInterceptor
{
    private readonly ILogger<DatabaseErrorLoggingInterceptor> _logger;

    public DatabaseErrorLoggingInterceptor(ILogger<DatabaseErrorLoggingInterceptor> logger)
    {
        _logger = logger;
    }

    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
    {
        LogFailure(eventData);
        base.CommandFailed(command, eventData);
    }

    public override Task CommandFailedAsync(
        DbCommand command,
        CommandErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        LogFailure(eventData);
        return base.CommandFailedAsync(command, eventData, cancellationToken);
    }

    private void LogFailure(CommandErrorEventData eventData)
    {
        _logger.LogError(
            eventData.Exception,
            "Database command failed for provider {DatabaseProvider}.",
            eventData.Context?.Database.ProviderName ?? "unknown");
    }
}
