using Microsoft.AspNetCore.Identity;
using StudentCourseRegistration.Api.Application.Abstractions.Security;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Infrastructure.Security;

/// <summary>Delegates password verification to ASP.NET Core Identity's secure hasher.</summary>
public sealed class AspNetPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<Student> _hasher = new();

    public bool Verify(Student student, string password) =>
        _hasher.VerifyHashedPassword(student, student.PasswordHash, password) != PasswordVerificationResult.Failed;
}
