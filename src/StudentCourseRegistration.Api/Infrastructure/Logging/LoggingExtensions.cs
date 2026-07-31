using Microsoft.Extensions.Logging.Console;
using System.Text.Json;

namespace StudentCourseRegistration.Api.Infrastructure.Logging;

/// <summary>Configures the application's structured console logging pipeline.</summary>
public static class LoggingExtensions
{
    /// <summary>Writes structured application logs as JSON to the console.</summary>
    public static ILoggingBuilder AddStructuredConsoleLogging(this ILoggingBuilder logging)
    {
        logging.ClearProviders();
        logging.AddJsonConsole(options => options.JsonWriterOptions = new JsonWriterOptions { Indented = false });
        return logging;
    }
}
