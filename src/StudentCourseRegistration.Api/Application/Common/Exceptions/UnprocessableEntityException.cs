namespace StudentCourseRegistration.Api.Application.Common.Exceptions;

/// <summary>Indicates that a request is well-formed but cannot be processed under current rules (HTTP 422).</summary>
public sealed class UnprocessableEntityException : Exception
{
    public UnprocessableEntityException(string message) : base(message)
    {
    }
}
