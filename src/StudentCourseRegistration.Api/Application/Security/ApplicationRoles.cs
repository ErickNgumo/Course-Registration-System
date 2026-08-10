namespace StudentCourseRegistration.Api.Application.Security;

/// <summary>Canonical authorization role names used across the system.</summary>
public static class ApplicationRoles
{
    /// <summary>The role granted to authenticated students.</summary>
    public const string Student = "Student";

    /// <summary>The role granted to authenticated administrators.</summary>
    public const string Administrator = "Administrator";
}
