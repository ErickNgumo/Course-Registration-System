using Moq;
using StudentCourseRegistration.Api.Application.Common.Exceptions;
using StudentCourseRegistration.Api.Domain.Courses;
using StudentCourseRegistration.Api.Domain.Enrollments;
using StudentCourseRegistration.Api.Domain.Students;
using StudentCourseRegistration.Tests.Testing;
using Xunit;

namespace StudentCourseRegistration.Tests.Application.Enrollments;

/// <summary>Enrollment list and dashboard scenarios for the enrollment service.</summary>
public sealed class EnrollmentServiceReadTests : EnrollmentServiceTestBase
{
    [Fact]
    public async Task GetEnrollmentsAsync_ReturnsMappedEnrollmentsIncludingDropped()
    {
        var completedCourseId = Guid.NewGuid();
        var registeredCourseId = Guid.NewGuid();
        var droppedCourseId = Guid.NewGuid();

        var enrollments = new List<Enrollment>
        {
            Fixtures.Enrollment(StudentId, completedCourseId, EnrollmentStatus.Completed, finalGrade: "A"),
            Fixtures.Enrollment(StudentId, registeredCourseId, EnrollmentStatus.Registered),
            Fixtures.Enrollment(StudentId, droppedCourseId, EnrollmentStatus.Dropped)
        };

        Enrollments.Setup(repository => repository.GetStudentEnrollmentsAsync(StudentId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollments);

        var completedCourse = Fixtures.Course(completedCourseId, "CSC101", "Intro", 3);
        var registeredCourse = Fixtures.Course(registeredCourseId, "CSC201", "Data Structures", 3);
        var droppedCourse = Fixtures.Course(droppedCourseId, "CSC999", "Dropped One", 3);
        Courses.Setup(repository => repository.FindByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, Course>
            {
                [completedCourseId] = completedCourse,
                [registeredCourseId] = registeredCourse,
                [droppedCourseId] = droppedCourse
            });

        var result = await Service.GetEnrollmentsAsync(StudentId, CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Contains(result, e => e.Status == EnrollmentStatus.Completed && e.FinalGrade == "A");
        Assert.Contains(result, e => e.Status == EnrollmentStatus.Registered);
        Assert.Contains(result, e => e.Status == EnrollmentStatus.Dropped);
    }

    [Fact]
    public async Task GetEnrollmentsAsync_ReturnsEmptyList_WhenStudentHasNoEnrollments()
    {
        Enrollments.Setup(repository => repository.GetStudentEnrollmentsAsync(StudentId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Enrollment>());

        var result = await Service.GetEnrollmentsAsync(StudentId, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetEnrollmentsAsync_PropagatesRepositoryFailure()
    {
        Enrollments.Setup(repository => repository.GetStudentEnrollmentsAsync(StudentId, true, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable."));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Service.GetEnrollmentsAsync(StudentId, CancellationToken.None));
    }

    [Fact]
    public async Task GetDashboardAsync_GroupsEnrollmentsByStatusAndSumsCredits()
    {
        var student = Fixtures.Student(StudentId);
        Students.Setup(repository => repository.FindByIdAsync(StudentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        var registeredCourseId = Guid.NewGuid();
        var waitlistedCourseId = Guid.NewGuid();
        var completedCourseId = Guid.NewGuid();

        var enrollments = new List<Enrollment>
        {
            Fixtures.Enrollment(StudentId, registeredCourseId, EnrollmentStatus.Registered),
            Fixtures.Enrollment(StudentId, waitlistedCourseId, EnrollmentStatus.Waitlisted),
            Fixtures.Enrollment(StudentId, completedCourseId, EnrollmentStatus.Completed, finalGrade: "B")
        };

        Enrollments.Setup(repository => repository.GetStudentEnrollmentsAsync(StudentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollments);

        var registeredCourse = Fixtures.Course(registeredCourseId, "CSC201", "Data Structures", 3, semester: "Fall 2026");
        var waitlistedCourse = Fixtures.Course(waitlistedCourseId, "CSC204", "Operating Systems", 3, semester: "Fall 2026");
        var completedCourse = Fixtures.Course(completedCourseId, "CSC101", "Intro", 3, semester: "Spring 2026");
        Courses.Setup(repository => repository.FindByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, Course>
            {
                [registeredCourseId] = registeredCourse,
                [waitlistedCourseId] = waitlistedCourse,
                [completedCourseId] = completedCourse
            });

        var dashboard = await Service.GetDashboardAsync(StudentId, CancellationToken.None);

        Assert.Equal(StudentId, dashboard.Student.Id);
        Assert.Single(dashboard.RegisteredCourses);
        Assert.Single(dashboard.WaitlistedCourses);
        Assert.Single(dashboard.CompletedCourses);
        Assert.Equal(3, dashboard.CurrentSemesterCredits); // only Fall 2026 registered course
        Assert.Equal(Options.MaxSemesterCredits, dashboard.MaxSemesterCredits);
    }

    [Fact]
    public async Task GetDashboardAsync_ReportsZeroCredits_WhenNoRegisteredCourses()
    {
        var student = Fixtures.Student(StudentId);
        Students.Setup(repository => repository.FindByIdAsync(StudentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);
        Enrollments.Setup(repository => repository.GetStudentEnrollmentsAsync(StudentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Enrollment>());

        var dashboard = await Service.GetDashboardAsync(StudentId, CancellationToken.None);

        Assert.Equal(0, dashboard.CurrentSemesterCredits);
        Assert.Empty(dashboard.RegisteredCourses);
        Assert.Empty(dashboard.WaitlistedCourses);
        Assert.Empty(dashboard.CompletedCourses);
    }

    [Fact]
    public async Task GetDashboardAsync_ThrowsNotFound_WhenStudentDoesNotExist()
    {
        Students.Setup(repository => repository.FindByIdAsync(StudentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Service.GetDashboardAsync(StudentId, CancellationToken.None));
    }

    [Fact]
    public async Task GetDashboardAsync_PropagatesRepositoryFailure()
    {
        Students.Setup(repository => repository.FindByIdAsync(StudentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable."));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Service.GetDashboardAsync(StudentId, CancellationToken.None));
    }
}
