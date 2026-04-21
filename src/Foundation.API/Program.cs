using Foundation.API.Endpoints;
using Foundation.API.Extensions;
using Foundation.API.Requests;
using Foundation.Application.Extensions;
using Foundation.Application.Services;
using Foundation.Infrastructure.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using System.Security.Claims;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Observability
var appInsightsKey = builder.Configuration["Observability:ApplicationInsights:InstrumentationKey"] ?? string.Empty;
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["Observability:ApplicationInsights:ConnectionString"];
    options.InstrumentationKey = appInsightsKey;
});

// Database & services
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

// Authentication & Authorization
var azureAdSettings = builder.Configuration.GetSection("Authentication:AzureAd");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = azureAdSettings["Authority"];
        options.Audience = azureAdSettings["Audience"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EmployeePolicy", policy => policy.RequireRole("Employee"));
    options.AddPolicy("LeadershipPolicy", policy => policy.RequireRole("Leadership"));
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    options.AddPolicy("TaxonomyAdminPolicy", policy => policy.RequireRole("Taxonomy Admin"));
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://app.corp.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("Fixed", _ =>
        RateLimitPartition.GetTokenBucketLimiter("Global", key => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 100,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            TokensPerPeriod = 100,
            AutoReplenishment = true
        }));
});

var app = builder.Build();

app.UseRateLimiter();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health/live", () => Results.Ok(new { status = "Healthy" }));
app.MapGet("/health/ready", ([FromServices] Foundation.Infrastructure.Data.FoundationDbContext context) =>
{
    var canConnect = context.Database.CanConnect();
    return canConnect ? Results.Ok(new { status = "Ready" }) : Results.StatusCode(503);
});

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new { name = e.Key, status = e.Value.Status.ToString() })
        });
    }
});

app.MapGet("/metrics", () => Results.Ok("Metrics endpoint"));

app.MapGet("/employees", [Authorize(Policy = "EmployeePolicy")] async ([FromServices] IEmployeeService service) =>
    Results.Ok(await service.ListAsync()));

app.MapGet("/employees/{id}", [Authorize(Policy = "LeadershipPolicy")] async ([FromServices] IEmployeeService service, Guid id) =>
    await service.GetAsync(id) switch
    {
        { } employee => Results.Ok(employee),
        null => Results.NotFound()
    });

app.MapPost("/employees", [Authorize(Policy = "AdminPolicy")] ([FromServices] IEmployeeService service, [FromBody] Foundation.Domain.Entities.Employee employee) =>
{
    var task = service.GetAsync(employee.Id);
    return Results.Accepted();
});

app.MapGet("/skills", [Authorize(Policy = "EmployeePolicy")] async ([FromServices] ISkillService service) =>
    Results.Ok(await service.ListAsync()));

app.MapGet("/skills/primary/{primarySkillId}/secondary", [Authorize(Policy = "EmployeePolicy")] async ([FromServices] ISkillService service, Guid primarySkillId) =>
    Results.Ok(await service.GetSecondarySkillsAsync(primarySkillId)));

app.MapPost("/skills/primary", [Authorize(Policy = "TaxonomyAdminPolicy")] async ([FromServices] ISkillService service, [FromBody] CreatePrimarySkillRequest request) =>
{
    var skill = await service.CreatePrimarySkillAsync(request.Name, request.Category);
    return Results.Created($"/skills/{skill.Id}", skill);
});

app.MapPost("/skills/secondary", [Authorize(Policy = "TaxonomyAdminPolicy")] async ([FromServices] ISkillService service, [FromBody] CreateSecondarySkillRequest request) =>
{
    var skill = await service.CreateSecondarySkillAsync(request.Name, request.Category, request.PrimarySkillId);
    return Results.Created($"/skills/{skill.Id}", skill);
});

app.MapPost("/skills/deactivate", [Authorize(Policy = "TaxonomyAdminPolicy")] async ([FromServices] ISkillService service, [FromBody] DeactivateSkillRequest request) =>
{
    await service.DeactivateSkillAsync(request.SkillId);
    return Results.NoContent();
});

app.Run();
