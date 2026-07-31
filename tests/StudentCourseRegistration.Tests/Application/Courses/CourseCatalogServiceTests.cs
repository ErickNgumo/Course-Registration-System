using Xunit;
using Moq;
using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Application.Common.Exceptions;
using StudentCourseRegistration.Api.Application.Courses;
using StudentCourseRegistration.Api.Domain.Courses;

namespace StudentCourseRegistration.Tests.Application.Courses;

public sealed class CourseCatalogServiceTests
{
    private readonly Mock<ICourseRepository> _courses = new();
    private readonly CourseCatalogService _service;

    public CourseCatalogServiceTests()
    {
        _service = new CourseCatalogService(_courses.Object);
    }

    [Fact]
    public async Task GetActiveCoursesAsync_ReturnsOnlyMappedActiveCourses()
    {
        var course = CreateCourse();
        var inactiveCourse = CreateCourse(isActive: false);
        _courses.Setup(repository => repository.GetActiveCoursesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Course> { course, inactiveCourse });

        var result = await _service.GetActiveCoursesAsync(CancellationToken.None);

        var returnedCourse = Assert.Single(result);
        Assert.Equal(course.Id, returnedCourse.Id);
        Assert.Equal(course.Code, returnedCourse.Code);
        Assert.Equal(course.Semester, returnedCourse.Semester);
    }

    [Fact]
    public async Task GetActiveCoursesAsync_PropagatesRepositoryFailure()
    {
        _courses.Setup(repository => repository.GetActiveCoursesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable."));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GetActiveCoursesAsync(CancellationToken.None));
    }

    [Fact]
    public async Task GetActiveCourseAsync_ReturnsMappedCourse_WhenCourseIsActive()
    {
        var course = CreateCourse();
        _courses.Setup(repository => repository.FindByIdAsync(course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        var result = await _service.GetActiveCourseAsync(course.Id, CancellationToken.None);

        Assert.Equal(course.Id, result.Id);
        Assert.Equal(course.Description, result.Description);
    }

    [Fact]
    public async Task GetActiveCourseAsync_ThrowsNotFoundException_WhenCourseDoesNotExist()
    {
        _courses.Setup(repository => repository.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Course?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetActiveCourseAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task GetActiveCourseAsync_ThrowsNotFoundException_WhenCourseIsInactive()
    {
        var course = CreateCourse(isActive: false);
        _courses.Setup(repository => repository.FindByIdAsync(course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetActiveCourseAsync(course.Id, CancellationToken.None));
    }

    [Fact]
    public async Task GetActiveCourseAsync_PropagatesRepositoryFailure()
    {
        _courses.Setup(repository => repository.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable."));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GetActiveCourseAsync(Guid.NewGuid(), CancellationToken.None));
    }

    private static Course CreateCourse(bool isActive = true) => new()
    {
        Id = Guid.NewGuid(),
        Code = "CSC101",
        Name = "Introduction to Programming",
        Description = "Foundational programming concepts.",
        Credits = 3,
        Capacity = 50,
        Semester = "Fall 2026",
        IsActive = isActive,
        CreatedAt = DateTimeOffset.UtcNow
    };
}
