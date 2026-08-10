using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudentCourseRegistration.Api.Api.Middleware;
using StudentCourseRegistration.Api.Api.Security;
using StudentCourseRegistration.Api.Application.Abstractions.Persistence;
using StudentCourseRegistration.Api.Application.Abstractions.Security;
using StudentCourseRegistration.Api.Application.Admin;
using StudentCourseRegistration.Api.Application.Audit;
using StudentCourseRegistration.Api.Application.Auth;
using StudentCourseRegistration.Api.Application.Courses;
using StudentCourseRegistration.Api.Application.Enrollments;
using StudentCourseRegistration.Api.Application.Security;
using StudentCourseRegistration.Api.Domain.Administrators;
using StudentCourseRegistration.Api.Domain.Students;
using StudentCourseRegistration.Api.Infrastructure.Logging;
using StudentCourseRegistration.Api.Infrastructure.Persistence;
using StudentCourseRegistration.Api.Infrastructure.Persistence.Interceptors;
using StudentCourseRegistration.Api.Infrastructure.Persistence.Repositories;
using StudentCourseRegistration.Api.Infrastructure.Persistence.Seed;
using StudentCourseRegistration.Api.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddStructuredConsoleLogging();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is required.");

builder.Services.AddSingleton<DatabaseErrorLoggingInterceptor>();
builder.Services.AddDbContext<RegistrationDbContext>((serviceProvider, options) =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RegistrationDatabase"))
        .AddInterceptors(serviceProvider.GetRequiredService<DatabaseErrorLoggingInterceptor>()));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.Administrator, policy =>
        policy.RequireRole(ApplicationRoles.Administrator));
    options.AddPolicy(AuthorizationPolicies.Student, policy =>
        policy.RequireRole(ApplicationRoles.Student));
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1.0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });
builder.Services.AddHealthChecks().AddDbContextCheck<RegistrationDbContext>("sqlserver");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<ISchedulingRepository, SchedulingRepository>();
builder.Services.AddScoped<IPrerequisiteRepository, PrerequisiteRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.Configure<EnrollmentOptions>(builder.Configuration.GetSection(EnrollmentOptions.SectionName));
builder.Services.AddSingleton<IPasswordHasher, AspNetPasswordHasher>();
builder.Services.AddSingleton<IPasswordHasher<Administrator>, IdentityPasswordHasher>();
builder.Services.AddSingleton<Microsoft.AspNetCore.Identity.IPasswordHasher<Administrator>, Microsoft.AspNetCore.Identity.PasswordHasher<Administrator>>();
builder.Services.AddSingleton<Microsoft.AspNetCore.Identity.IPasswordHasher<Student>, Microsoft.AspNetCore.Identity.PasswordHasher<Student>>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ICourseCatalogService, CourseCatalogService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// Administration services
builder.Services.AddScoped<IAdministratorRepository, AdministratorRepository>();
builder.Services.AddScoped<ICourseAdministrationRepository, CourseAdministrationRepository>();
builder.Services.AddScoped<IEnrollmentAdministrationRepository, EnrollmentAdministrationRepository>();
builder.Services.AddScoped<IStudentAdministrationRepository, StudentAdministrationRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IAuditRecorder, AuditRecorder>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAdministrationService, AdministrationService>();
builder.Services.AddScoped<IAdministratorAuthenticationService, AdministratorAuthenticationService>();
builder.Services.AddScoped<ICourseAdministrationService, CourseAdministrationService>();
builder.Services.AddScoped<IEnrollmentAdministrationService, EnrollmentAdministrationService>();
builder.Services.AddScoped<IStudentAdministrationService, StudentAdministrationService>();
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddScoped<DevelopmentDatabaseSeeder>();


var app = builder.Build();

app.Logger.LogInformation("Starting Student Course Registration API in {Environment} environment.", app.Environment.EnvironmentName);

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
    var seeder = scope.ServiceProvider.GetRequiredService<DevelopmentDatabaseSeeder>();

    await context.Database.MigrateAsync();
    await seeder.SeedAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

app.Run();

public partial class Program;
