using Xunit;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Application.Abstractions.Security;
using StudentCourseRegistration.Api.Application.Auth;
using StudentCourseRegistration.Api.Application.Common.Exceptions;
using StudentCourseRegistration.Api.Domain.Students;

namespace StudentCourseRegistration.Tests.Application.Auth;

public sealed class AuthenticationServiceTests
{
    private readonly Mock<IStudentRepository> _students = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<ITokenService> _tokens = new();
    private readonly AuthenticationService _service;

    public AuthenticationServiceTests()
    {
        _service = new AuthenticationService(
            _students.Object,
            _passwordHasher.Object,
            _tokens.Object,
            NullLogger<AuthenticationService>.Instance);
    }

    [Fact]
    public async Task LoginAsync_ReturnsTokenAndStudent_WhenCredentialsAreValid()
    {
        var student = CreateStudent();
        _students.Setup(repository => repository.FindByEmailAsync("JANE@UNIVERSITY.EDU", It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);
        _passwordHasher.Setup(hasher => hasher.Verify(student, "password")).Returns(true);
        _tokens.Setup(service => service.CreateAccessToken(student))
            .Returns(new AccessToken("token", DateTimeOffset.Parse("2026-07-17T11:00:00Z")));

        var result = await _service.LoginAsync(new LoginCommand(" jane@university.edu ", "password"), CancellationToken.None);

        Assert.Equal("token", result.AccessToken);
        Assert.Equal("Bearer", result.TokenType);
        Assert.InRange(result.ExpiresIn, 0, 1);
        Assert.Equal(student.Id, result.Student.Id);
        Assert.Equal(student.Email, result.Student.Email);
    }

    [Fact]
    public async Task LoginAsync_ThrowsAuthenticationException_WhenStudentDoesNotExist()
    {
        _students.Setup(repository => repository.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            _service.LoginAsync(new LoginCommand("jane@university.edu", "password"), CancellationToken.None));

        _passwordHasher.Verify(hasher => hasher.Verify(It.IsAny<Student>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ThrowsAuthenticationException_WhenPasswordIsInvalid()
    {
        var student = CreateStudent();
        _students.Setup(repository => repository.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);
        _passwordHasher.Setup(hasher => hasher.Verify(student, "wrong-password")).Returns(false);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            _service.LoginAsync(new LoginCommand("jane@university.edu", "wrong-password"), CancellationToken.None));
    }

    [Theory]
    [InlineData(StudentStatus.Inactive)]
    [InlineData(StudentStatus.Suspended)]
    public async Task LoginAsync_ThrowsForbiddenException_WhenStudentIsNotActive(StudentStatus status)
    {
        var student = CreateStudent(status);
        _students.Setup(repository => repository.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);
        _passwordHasher.Setup(hasher => hasher.Verify(student, "password")).Returns(true);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.LoginAsync(new LoginCommand("jane@university.edu", "password"), CancellationToken.None));

        _tokens.Verify(service => service.CreateAccessToken(It.IsAny<Student>()), Times.Never);
    }

    [Fact]
    public async Task GetCurrentStudentAsync_ReturnsStudent_WhenAccountIsActive()
    {
        var student = CreateStudent();
        _students.Setup(repository => repository.FindByIdAsync(student.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        var result = await _service.GetCurrentStudentAsync(student.Id, CancellationToken.None);

        Assert.Equal(student.Id, result.Id);
        Assert.Equal(student.StudentNumber, result.StudentNumber);
    }

    [Fact]
    public async Task GetCurrentStudentAsync_ThrowsNotFoundException_WhenStudentDoesNotExist()
    {
        _students.Setup(repository => repository.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetCurrentStudentAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task GetCurrentStudentAsync_ThrowsForbiddenException_WhenStudentIsNotActive()
    {
        var student = CreateStudent(StudentStatus.Suspended);
        _students.Setup(repository => repository.FindByIdAsync(student.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.GetCurrentStudentAsync(student.Id, CancellationToken.None));
    }

    private static Student CreateStudent(StudentStatus status = StudentStatus.Active) => new()
    {
        Id = Guid.NewGuid(),
        StudentNumber = "STU-2026-001",
        FirstName = "Jane",
        LastName = "Doe",
        Email = "jane@university.edu",
        PasswordHash = "hash",
        Status = status
    };
}
