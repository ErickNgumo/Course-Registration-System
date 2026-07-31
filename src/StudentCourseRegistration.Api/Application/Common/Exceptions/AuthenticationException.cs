namespace StudentCourseRegistration.Api.Application.Common.Exceptions;

/// <summary>Indicates that supplied credentials are not valid for authentication.</summary>
public sealed class AuthenticationException : Exception
{
    public AuthenticationException() : base("The email address or password is incorrect.")
    {
    }
}
