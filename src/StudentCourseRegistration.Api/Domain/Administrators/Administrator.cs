namespace StudentCourseRegistration.Api.Domain.Administrators;

/// <summary>Represents a development administrator account reserved for future administration features.</summary>
public sealed class Administrator
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
