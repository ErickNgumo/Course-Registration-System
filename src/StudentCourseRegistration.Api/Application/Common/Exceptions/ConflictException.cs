namespace StudentCourseRegistration.Api.Application.Common.Exceptions;

/// <summary>Indicates that a request conflicts with the current state of the resource (HTTP 409).</summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
