namespace StudentCourseRegistration.Api.Application.Admin;

/// <summary>Assembles the administration dashboard.</summary>
public interface IAdministrationService
{
    /// <summary>Returns the consolidated administration dashboard aggregates.</summary>
    Task<AdministratorDashboardDto> GetDashboardAsync(CancellationToken cancellationToken);
}
