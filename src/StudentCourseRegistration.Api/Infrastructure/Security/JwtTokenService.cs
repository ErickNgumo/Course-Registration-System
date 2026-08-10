using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StudentCourseRegistration.Api.Application.Abstractions.Security;
using StudentCourseRegistration.Api.Application.Security;
using StudentCourseRegistration.Api.Domain.Administrators;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Infrastructure.Security;

/// <summary>Creates short-lived signed JWT access tokens for active student and administrator accounts.</summary>
public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public AccessToken CreateAccessToken(Student student) => CreateAccessToken(student, ApplicationRoles.Student);

    /// <inheritdoc />
    public AccessToken CreateAccessToken(Student student, string role) =>
        Create(
            subjectId: student.Id,
            email: student.Email,
            role: role,
            extra: new Claim("student_number", student.StudentNumber));

    /// <inheritdoc />
    public AccessToken CreateAccessToken(Administrator administrator) =>
        Create(
            subjectId: administrator.Id,
            email: administrator.Email,
            role: ApplicationRoles.Administrator,
            extra: null);

    private AccessToken Create(Guid subjectId, string email, string role, Claim? extra)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.ExpiresInMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subjectId.ToString()),
            new(ClaimTypes.NameIdentifier, subjectId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role)
        };
        if (extra is not null)
        {
            claims.Add(extra);
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
