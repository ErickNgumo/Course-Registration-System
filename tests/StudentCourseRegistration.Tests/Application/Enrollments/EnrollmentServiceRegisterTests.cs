using Moq;
using StudentCourseRegistration.Api.Application.Common.Exceptions;
using StudentCourseRegistration.Api.Domain.Courses;
using StudentCourseRegistration.Api.Domain.Enrollments;
using StudentCourseRegistration.Tests.Testing;
using Xunit;

namespace StudentCourseRegistration.Tests.Application.Enrollments;

/// <summary>Register and drop scenarios for the enrollment service.</summary>
public sealed class EnrollmentServiceRegisterTests : EnrollmentServiceTestBase
{
    [Fact]
    public async Task RegisterAsync_RegistersStudent_WhenAllRulesPass()
    {
        var course = DefaultCourse();
        Enrollments.Setup(repository => repository.FindActiveAsync(StudentId, course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);
        Prerequisites.Setup(repository => repository.GetPrerequisiteCourseIdsAsync(course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Guid>());
        Schedules.Setup(repository => repository.GetSchedulesForCoursesAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IReadOnlyList<CourseSchedule>>());
        Enrollments.Setup(repository => repository.CountRegisteredAsync(course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        Enrollments.Setup(repository => repository.SumRegisteredCreditsAsync(StudentId, course.Semester, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        Enrollments.Setup(repository => repository.UpsertAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment enroll, CancellationToken _) => enroll);

        var result = await Service.RegisterAsync(StudentId, course.Id, CancellationToken.None);

        Assert.Equal(EnrollmentStatus.Registered, result.Status);
        Assert.Equal(course.Id, result.CourseId);
    }

    [Fact]
    public async Task RegisterAsync_ThrowsNotFound_WhenCourseDoesNotExist()
    {
        Courses.Setup(repository => repository.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Course?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Service.RegisterAsync(StudentId, Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_ThrowsUnprocessable_WhenCourseIsInactive()
    {
        var course = DefaultCourse(active: false);

        await Assert.ThrowsAsync<UnprocessableEntityException>(() =>
            Service.RegisterAsync(StudentId, course.Id, CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_ThrowsConflict_OnDuplicateActiveRegistration()
    {
        var course = DefaultCourse();
        var existing = Fixtures.Enrollment(StudentId, course.Id, EnrollmentStatus.Registered);
        Enrollments.Setup(repository => repository.FindActiveAsync(StudentId, course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await Assert.ThrowsAsync<ConflictException>(() =>
            Service.RegisterAsync(StudentId, course.Id, CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_ThrowsConflict_OnExistingWaitlistedEnrollment()
    {
        var course = DefaultCourse();
        var existing = Fixtures.Enrollment(StudentId, course.Id, EnrollmentStatus.Waitlisted);
        Enrollments.Setup(repository => repository.FindActiveAsync(StudentId, course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await Assert.ThrowsAsync<ConflictException>(() =>
            Service.RegisterAsync(StudentId, course.Id, CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_ThrowsUnprocessable_WhenPrerequisiteNotCompleted()
    {
        var course = DefaultCourse();
        var prerequisiteId = Guid.NewGuid();
        Enrollments.Setup(repository => repository.FindActiveAsync(StudentId, course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);
        Prerequisites.Setup(repository => repository.GetPrerequisiteCourseIdsAsync(course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid> { prerequisiteId });
        Enrollments.Setup(repository => repository.FindCompletedAsync(StudentId, prerequisiteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);

        await Assert.ThrowsAsync<UnprocessableEntityException>(() =>
            Service.RegisterAsync(StudentId, course.Id, CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_Registers_WhenPrerequisiteIsCompleted()
    {
        var course = DefaultCourse();
        var prerequisiteId = Guid.NewGuid();
        Enrollments.Setup(repository => repository.FindActiveAsync(StudentId, course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);
        Prerequisites.Setup(repository => repository.GetPrerequisiteCourseIdsAsync(course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid> { prerequisiteId });
        Enrollments.Setup(repository => repository.FindCompletedAsync(StudentId, prerequisiteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Fixtures.Enrollment(StudentId, prerequisiteId, EnrollmentStatus.Completed));
        Schedules.Setup(repository => repository.GetSchedulesForCoursesAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IReadOnlyList<CourseSchedule>>());
        Enrollments.Setup(repository => repository.CountRegisteredAsync(course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        Enrollments.Setup(repository => repository.SumRegisteredCreditsAsync(StudentId, course.Semester, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        Enrollments.Setup(repository => repository.UpsertAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment enroll, CancellationToken _) => enroll);

        var result = await Service.RegisterAsync(StudentId, course.Id, CancellationToken.None);

        Assert.Equal(EnrollmentStatus.Registered, result.Status);
    }

    [Fact]
    public async Task RegisterAsync_ThrowsUnprocessable_OnTimetableConflict()
    {
        var course = DefaultCourse();
        var existingCourseId = Guid.NewGuid();
        Enrollments.Setup(repository => repository.FindActiveAsync(StudentId, course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);
        Prerequisites.Setup(repository => repository.GetPrerequisiteCourseIdsAsync(course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Guid>());

        var registered = Fixtures.Enrollment(StudentId, existingCourseId, EnrollmentStatus.Registered);
        Enrollments.Setup(repository => repository.GetStudentEnrollmentsAsync(StudentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Enrollment> { registered });

        var schedules = new Dictionary<Guid, IReadOnlyList<CourseSchedule>>
        {
            [course.Id] = new[] { Slot(course.Id, DayOfWeek.Monday, "09:00", "10:30") },
            [existingCourseId] = new[] { Slot(existingCourseId, DayOfWeek.Monday, "10:00", "11:30") }
        };
        Schedules.Setup(repository => repository.GetSchedulesForCoursesAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedules);

        await Assert.ThrowsAsync<UnprocessableEntityException>(() =>
            Service.RegisterAsync(StudentId, course.Id, CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_Registers_WhenNoTimetableConflict()
    {
        var course = DefaultCourse();
        var existingCourseId = Guid.NewGuid();
        Enrollments.Setup(repository => repository.FindActiveAsync(StudentId, course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);
        Prerequisites.Setup(repository => repository.GetPrerequisiteCourseIdsAsync(course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Guid>());

        var registered = Fixtures.Enrollment(StudentId, existingCourseId, EnrollmentStatus.Registered);
        Enrollments.Setup(repository => repository.GetStudentEnrollmentsAsync(StudentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Enrollment> { registered });

        var schedules = new Dictionary<Guid, IReadOnlyList<CourseSchedule>>
        {
            [course.Id] = new[] { Slot(course.Id, DayOfWeek.Monday, "09:00", "10:30") },
            [existingCourseId] = new[] { Slot(existingCourseId, DayOfWeek.Tuesday, "09:00", "10:30") }
        };
        Schedules.Setup(repository => repository.GetSchedulesForCoursesAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedules);

        Enrollments.Setup(repository => repository.CountRegisteredAsync(course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        Enrollments.Setup(repository => repository.SumRegisteredCreditsAsync(StudentId, course.Semester, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        Enrollments.Setup(repository => repository.UpsertAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment enroll, CancellationToken _) => enroll);

        var result = await Service.RegisterAsync(StudentId, course.Id, CancellationToken.None);

        Assert.Equal(EnrollmentStatus.Registered, result.Status);
    }

    [Fact]
    public async Task RegisterAsync_ThrowsUnprocessable_WhenCreditLimitExceeded()
    {
        var course = DefaultCourse(credits: 6);
        Enrollments.Setup(repository => repository.FindActiveAsync(StudentId, course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);
        Prerequisites.Setup(repository => repository.GetPrerequisiteCourseIdsAsync(course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Guid>());
        Schedules.Setup(repository => repository.GetSchedulesForCoursesAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IReadOnlyList<CourseSchedule>>());
        Enrollments.Setup(repository => repository.CountRegisteredAsync(course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        Enrollments.Setup(repository => repository.SumRegisteredCreditsAsync(StudentId, course.Semester, It.IsAny<CancellationToken>()))
            .ReturnsAsync(18); // 18 + 6 = 24 > 21

        await Assert.ThrowsAsync<UnprocessableEntityException>(() =>
            Service.RegisterAsync(StudentId, course.Id, CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_WaitlistsStudent_WhenCourseIsFull()
    {
        var course = DefaultCourse(capacity: 1);
        Enrollments.Setup(repository => repository.FindActiveAsync(StudentId, course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);
        Prerequisites.Setup(repository => repository.GetPrerequisiteCourseIdsAsync(course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Guid>());
        Schedules.Setup(repository => repository.GetSchedulesForCoursesAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IReadOnlyList<CourseSchedule>>());
        Enrollments.Setup(repository => repository.CountRegisteredAsync(course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        Enrollments.Setup(repository => repository.SumRegisteredCreditsAsync(StudentId, course.Semester, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        Enrollments.Setup(repository => repository.UpsertAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment enroll, CancellationToken _) => enroll);

        var result = await Service.RegisterAsync(StudentId, course.Id, CancellationToken.None);

        Assert.Equal(EnrollmentStatus.Waitlisted, result.Status);
    }

    [Fact]
    public async Task RegisterAsync_ThrowsConflict_WhenCourseFullAndWaitlistDisabled()
    {
        Options.WaitlistEnabled = false;
        var course = DefaultCourse(capacity: 1);
        Enrollments.Setup(repository => repository.FindActiveAsync(StudentId, course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);
        Prerequisites.Setup(repository => repository.GetPrerequisiteCourseIdsAsync(course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Guid>());
        Schedules.Setup(repository => repository.GetSchedulesForCoursesAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IReadOnlyList<CourseSchedule>>());
        Enrollments.Setup(repository => repository.CountRegisteredAsync(course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        Enrollments.Setup(repository => repository.SumRegisteredCreditsAsync(StudentId, course.Semester, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        await Assert.ThrowsAsync<ConflictException>(() =>
            Service.RegisterAsync(StudentId, course.Id, CancellationToken.None));
    }

    [Fact]
    public async Task DropAsync_DropsRegisteredEnrollment_AndPromotesWaitlistedInTransaction()
    {
        var courseId = Guid.NewGuid();
        var enrollment = Fixtures.Enrollment(StudentId, courseId, EnrollmentStatus.Registered);
        Enrollments.Setup(repository => repository.FindByIdAsync(enrollment.Id, StudentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        Enrollments.Setup(repository => repository.UpsertAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment e, CancellationToken _) => e);

        var waiter = Fixtures.Enrollment(Guid.NewGuid(), courseId, EnrollmentStatus.Waitlisted);
        Enrollments.Setup(repository => repository.FindOldestWaitlistedAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(waiter);

        await Service.DropAsync(StudentId, enrollment.Id, CancellationToken.None);

        Assert.Equal(1, UnitOfWork.BeginCount);
        Assert.Equal(1, UnitOfWork.CommitCount);
        Enrollments.Verify(repository => repository.UpsertAsync(
            It.Is<Enrollment>(e => e.Id == waiter.Id && e.Status == EnrollmentStatus.Registered),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DropAsync_DropsWaitlistedEnrollment_WithoutPromoting()
    {
        var courseId = Guid.NewGuid();
        var enrollment = Fixtures.Enrollment(StudentId, courseId, EnrollmentStatus.Waitlisted);
        Enrollments.Setup(repository => repository.FindByIdAsync(enrollment.Id, StudentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        Enrollments.Setup(repository => repository.UpsertAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment e, CancellationToken _) => e);

        await Service.DropAsync(StudentId, enrollment.Id, CancellationToken.None);

        Enrollments.Verify(repository => repository.FindOldestWaitlistedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Equal(1, UnitOfWork.CommitCount);
    }

    [Fact]
    public async Task DropAsync_ThrowsNotFound_WhenEnrollmentDoesNotBelongToStudent()
    {
        Enrollments.Setup(repository => repository.FindByIdAsync(It.IsAny<Guid>(), StudentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Service.DropAsync(StudentId, Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task DropAsync_ThrowsConflict_WhenEnrollmentAlreadyDropped()
    {
        var enrollment = Fixtures.Enrollment(StudentId, Guid.NewGuid(), EnrollmentStatus.Dropped);
        Enrollments.Setup(repository => repository.FindByIdAsync(enrollment.Id, StudentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        await Assert.ThrowsAsync<ConflictException>(() =>
            Service.DropAsync(StudentId, enrollment.Id, CancellationToken.None));
    }

    [Fact]
    public async Task DropAsync_ThrowsConflict_WhenEnrollmentCompleted()
    {
        var enrollment = Fixtures.Enrollment(StudentId, Guid.NewGuid(), EnrollmentStatus.Completed);
        Enrollments.Setup(repository => repository.FindByIdAsync(enrollment.Id, StudentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        await Assert.ThrowsAsync<ConflictException>(() =>
            Service.DropAsync(StudentId, enrollment.Id, CancellationToken.None));
    }

    [Fact]
    public async Task DropAsync_RollsBackTransaction_OnRepositoryFailure()
    {
        var courseId = Guid.NewGuid();
        var enrollment = Fixtures.Enrollment(StudentId, courseId, EnrollmentStatus.Registered);
        Enrollments.Setup(repository => repository.FindByIdAsync(enrollment.Id, StudentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        Enrollments.Setup(repository => repository.UpsertAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable."));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Service.DropAsync(StudentId, enrollment.Id, CancellationToken.None));

        Assert.Equal(1, UnitOfWork.RollbackCount);
        Assert.Equal(0, UnitOfWork.CommitCount);
    }

    [Fact]
    public async Task RegisterAsync_PropagatesRepositoryFailure()
    {
        var course = DefaultCourse();
        Courses.Setup(repository => repository.FindByIdAsync(course.Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable."));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Service.RegisterAsync(StudentId, course.Id, CancellationToken.None));
    }

    [Fact]
    public async Task DropAsync_NoPromotion_WhenNoWaitlistedStudent()
    {
        var courseId = Guid.NewGuid();
        var enrollment = Fixtures.Enrollment(StudentId, courseId, EnrollmentStatus.Registered);
        Enrollments.Setup(repository => repository.FindByIdAsync(enrollment.Id, StudentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        Enrollments.Setup(repository => repository.UpsertAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment e, CancellationToken _) => e);
        Enrollments.Setup(repository => repository.FindOldestWaitlistedAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);

        await Service.DropAsync(StudentId, enrollment.Id, CancellationToken.None);

        Assert.Equal(1, UnitOfWork.CommitCount);
    }
}
