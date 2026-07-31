namespace StudentCourseRegistration.Api.Application.Common.Exceptions;

/// <summary>Indicates that an authenticated caller is not permitted to perform an operation.</summary>
public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
