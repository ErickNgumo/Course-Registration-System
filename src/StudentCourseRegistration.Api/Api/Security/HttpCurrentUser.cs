using System.Security.Claims;
using StudentCourseRegistration.Api.Application.Common.Exceptions;

namespace StudentCourseRegistration.Api.Api.Security;

public sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid StudentId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var studentId)
                ? studentId
                : throw new AuthenticationException();
        }
    }
}
