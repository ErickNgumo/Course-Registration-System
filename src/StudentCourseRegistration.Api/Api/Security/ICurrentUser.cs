namespace StudentCourseRegistration.Api.Api.Security;

public interface ICurrentUser
{
    Guid StudentId { get; }
}
