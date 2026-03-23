using Foundation.Application.DTOs;
using Foundation.Application.Extensions;
using Foundation.Application.Interfaces;
using Foundation.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
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

app.MapGet("/skills/primary", [Authorize(Policy = "EmployeePolicy")] async ([FromServices] ISkillService service, [FromQuery] bool? isActive) =>
{
    var skills = await service.GetPrimaryAsync(isActive);
    return Results.Ok(skills);
});

app.MapGet("/skills/{parentSkillId}/secondary", [Authorize(Policy = "EmployeePolicy")] async ([FromServices] ISkillService service, Guid parentSkillId, [FromQuery] bool? isActive) =>
{
    var skills = await service.GetSecondaryAsync(parentSkillId, isActive);
    return Results.Ok(skills);
});

app.MapPost("/skills", [Authorize(Policy = "AdminPolicy")] async ([FromServices] ISkillService service, HttpContext context, [FromBody] CreateSkillDto dto) =>
{
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

    try
    {
        var skill = await service.CreateAsync(dto, userId);
        return Results.Created($"/skills/{skill.Id}", skill);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(ex.Message);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(ex.Message);
    }
});

app.MapPut("/skills/{id}", [Authorize(Policy = "AdminPolicy")] async ([FromServices] ISkillService service, HttpContext context, Guid id, [FromBody] UpdateSkillDto dto) =>
{
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
    dto.Id = id;

    try
    {
        var updated = await service.UpdateAsync(dto, userId);
        return Results.Ok(updated);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(ex.Message);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(ex.Message);
    }
});

app.MapDelete("/skills/{id}", [Authorize(Policy = "AdminPolicy")] async ([FromServices] ISkillService service, HttpContext context, Guid id) =>
{
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

    try
    {
        await service.DeleteAsync(id, userId);
        return Results.NoContent();
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(ex.Message);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(ex.Message);
    }
});

app.Run();
