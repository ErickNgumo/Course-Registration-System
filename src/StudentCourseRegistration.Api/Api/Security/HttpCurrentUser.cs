using System.Security.Claims;
using StudentCourseRegistration.Api.Application.Common.Exceptions;
using StudentCourseRegistration.Api.Application.Security;

namespace StudentCourseRegistration.Api.Api.Security;

public sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id)
                ? id
                : throw new AuthenticationException();
        }
    }

    public Guid StudentId => UserId;

    public bool IsInRole(string role) => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<string> Roles
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            if (principal is null)
            {
                return Array.Empty<string>();
            }

            return principal.FindAll(ClaimTypes.Role).Select(claim => claim.Value).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
