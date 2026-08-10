namespace StudentCourseRegistration.Api.Application.Abstractions.Security;

/// <summary>Verifies a plaintext password against a stored hash for a given user type.</summary>
/// <typeparam name="TUser">The user entity type.</typeparam>
public interface IPasswordHasher<in TUser> where TUser : class
{
    /// <summary>Returns true when the supplied password matches the stored hash.</summary>
    bool Verify(TUser user, string password);
}
