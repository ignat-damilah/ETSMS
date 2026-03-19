using Foundation.Application.DTOs;
using Foundation.Application.Extensions;
using Foundation.Application.Services;
using Foundation.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
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

app.MapGet("/employees", [Authorize(Policy = "EmployeePolicy")] ([FromServices] IEmployeeService service) =>
    Results.Ok(service.ListAsync()));

app.MapGet("/employees/{id}", [Authorize(Policy = "LeadershipPolicy")] ([FromServices] IEmployeeService service, Guid id) =>
    service.GetAsync(id) switch
    {
        { } employee => Results.Ok(employee),
        null => Results.NotFound()
    });

app.MapPost("/employees", [Authorize(Policy = "AdminPolicy")] ([FromServices] IEmployeeService service, [FromBody] Foundation.Domain.Entities.Employee employee) =>
{
    var task = service.GetAsync(employee.Id);
    return Results.Accepted();
});

app.MapGet("/skills", [Authorize(Policy = "EmployeePolicy")] ([FromServices] ISkillService service) =>
    Results.Ok(service.ListAsync()));

app.MapGet("/skills/{id}", [Authorize(Policy = "LeadershipPolicy")] ([FromServices] ISkillService service, Guid id) =>
    service.GetAsync(id) switch
    {
        { } skill => Results.Ok(skill),
        null => Results.NotFound()
    });

app.MapGet("/skills/{primarySkillId}/secondaries", [Authorize(Policy = "EmployeePolicy")] ([FromServices] ISkillService service, Guid primarySkillId) =>
    Results.Ok(service.GetSecondarySkillsAsync(primarySkillId)));

app.MapPost("/skills", [Authorize(Policy = "AdminPolicy")] ([FromServices] ISkillService service, [FromBody] SkillCreateRequest request) =>
{
    service.CreateAsync(request);
    return Results.Accepted();
});

app.MapPut("/skills/{id}", [Authorize(Policy = "AdminPolicy")] async ([FromServices] ISkillService service, Guid id, [FromBody] SkillUpdateRequest request) =>
{
    if (id != request.Id)
    {
        return Results.BadRequest();
    }

    try
    {
        await service.UpdateAsync(request);
        return Results.NoContent();
    }
    catch (InvalidOperationException)
    {
        return Results.NotFound();
    }
});

app.MapDelete("/skills/{id}", [Authorize(Policy = "AdminPolicy")] async ([FromServices] ISkillService service, Guid id) =>
{
    try
    {
        await service.DeleteAsync(id);
        return Results.NoContent();
    }
    catch (InvalidOperationException)
    {
        return Results.NotFound();
    }
});

app.Run();