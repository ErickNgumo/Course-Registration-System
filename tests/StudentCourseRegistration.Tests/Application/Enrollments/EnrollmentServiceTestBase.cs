using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Application.Enrollments;
using StudentCourseRegistration.Api.Domain.Courses;
using StudentCourseRegistration.Api.Domain.Enrollments;
using StudentCourseRegistration.Api.Domain.Students;
using StudentCourseRegistration.Tests.Testing;

namespace StudentCourseRegistration.Tests.Application.Enrollments;

/// <summary>Shared repository mocks and service wiring for enrollment service tests.</summary>
public abstract class EnrollmentServiceTestBase
{
    protected readonly Mock<ICourseRepository> Courses = new();
    protected readonly Mock<IEnrollmentRepository> Enrollments = new();
    protected readonly Mock<IPrerequisiteRepository> Prerequisites = new();
    protected readonly Mock<ISchedulingRepository> Schedules = new();
    protected readonly Mock<IStudentRepository> Students = new();
    protected readonly FakeUnitOfWork UnitOfWork = new();
    protected readonly EnrollmentOptions Options = new() { MaxSemesterCredits = 21, WaitlistEnabled = true };
    protected readonly EnrollmentService Service;

    protected static readonly Guid StudentId = Guid.NewGuid();
    protected static readonly Guid CourseId = Guid.NewGuid();

    protected EnrollmentServiceTestBase()
    {
        Service = new EnrollmentService(
            Courses.Object,
            Enrollments.Object,
            Prerequisites.Object,
            Schedules.Object,
            Students.Object,
            UnitOfWork,
            Microsoft.Extensions.Options.Options.Create(Options),
            NullLogger<EnrollmentService>.Instance);
    }

    protected Course DefaultCourse(
        Guid? id = null,
        int capacity = 50,
        int credits = 3,
        bool active = true,
        string semester = "Fall 2026",
        string code = "CSC101",
        string name = "Introduction to Programming")
    {
        var course = Fixtures.Course(id ?? CourseId, code, name, credits, capacity, semester, active);
        Courses.Setup(repository => repository.FindByIdAsync(course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);
        Courses.Setup(repository => repository.FindByIdsAsync(It.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(course.Id)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, Course> { [course.Id] = course });
        return course;
    }

    protected static CourseSchedule Slot(Guid courseId, DayOfWeek day, string start, string end) =>
        Fixtures.Slot(courseId, day, TimeOnly.Parse(start), TimeOnly.Parse(end));
}
