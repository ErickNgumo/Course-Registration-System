namespace StudentCourseRegistration.Api.Application.Common.Exceptions;

/// <summary>Indicates that a requested resource does not exist.</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string resourceName) : base($"The requested {resourceName} was not found.")
    {
    }
}
