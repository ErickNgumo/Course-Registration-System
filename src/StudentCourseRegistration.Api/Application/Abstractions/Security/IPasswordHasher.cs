using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Api.Application.Abstractions.Security;

public interface IPasswordHasher
{
    bool Verify(Student student, string password);
}
