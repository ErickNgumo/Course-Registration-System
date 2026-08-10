namespace StudentCourseRegistration.Api.Application.Security;

/// <summary>Stable authorization policy names registered with the ASP.NET Core authorization pipeline.</summary>
public static class AuthorizationPolicies
{
    /// <summary>Requires an authenticated student.</summary>
    public const string Student = "Student";

    /// <summary>Requires an authenticated administrator.</summary>
    public const string Administrator = "Administrator";
}
