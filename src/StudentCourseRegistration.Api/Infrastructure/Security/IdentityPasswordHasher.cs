using Microsoft.AspNetCore.Identity;
using StudentCourseRegistration.Api.Domain.Administrators;

namespace StudentCourseRegistration.Api.Infrastructure.Security;

/// <summary>Delegates administrator password verification to ASP.NET Core Identity's secure hasher.</summary>
public sealed class IdentityPasswordHasher : StudentCourseRegistration.Api.Application.Abstractions.Security.IPasswordHasher<Administrator>
{
    private readonly PasswordHasher<Administrator> _hasher = new();

    /// <inheritdoc />
    public bool Verify(Administrator administrator, string password) =>
        _hasher.VerifyHashedPassword(administrator, administrator.PasswordHash, password) != PasswordVerificationResult.Failed;
}
