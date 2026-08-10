using StudentCourseRegistration.Api.Application.Security;

namespace StudentCourseRegistration.Api.Api.Security;

/// <summary>Exposes the authenticated caller's identity and roles to application and admin services.</summary>
public interface ICurrentUser
{
    /// <summary>The identifier of the authenticated principal (student or administrator).</summary>
    Guid UserId { get; }

    /// <summary>Back-compat alias of <see cref="UserId"/> for student-facing endpoints.</summary>
    Guid StudentId { get; }

    /// <summary>Returns true when the authenticated principal belongs to the specified role.</summary>
    bool IsInRole(string role);

    /// <summary>The roles assigned to the authenticated principal.</summary>
    IReadOnlyList<string> Roles { get; }
}
