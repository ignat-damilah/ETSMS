using System.Security.Claims;
using Foundation.API.Endpoints;
using Foundation.Application.DTOs;
using Foundation.Application.Extensions;
using Foundation.Application.Services;
using Foundation.Infrastructure.Extensions;
using Foundation.Infrastructure.Repositories;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Observability
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["Observability:ApplicationInsights:ConnectionString"];
    options.InstrumentationKey = builder.Configuration["Observability:ApplicationInsights:InstrumentationKey"];
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
    options.AddPolicy("SkillManagerPolicy", policy => policy.RequireRole("SkillManager"));
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://app.corp.com");
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.AllowCredentials();
    });
});

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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Foundation API", Version = "v1" });
});

var app = builder.Build();

app.UseRateLimiter();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

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

app.MapGet("/employees", [Authorize(Policy = "EmployeePolicy")] ([FromServices] Foundation.Application.Services.IEmployeeService service) =>
    Results.Ok(service.ListAsync()));

app.MapGet("/employees/{id}", [Authorize(Policy = "LeadershipPolicy")] ([FromServices] Foundation.Application.Services.IEmployeeService service, Guid id) =>
    service.GetAsync(id) switch
    {
        { } employee => Results.Ok(employee),
        null => Results.NotFound()
    });

app.MapPost("/employees", [Authorize(Policy = "AdminPolicy")] ([FromServices] Foundation.Application.Services.IEmployeeService service, [FromBody] Foundation.Domain.Entities.Employee employee) =>
{
    var task = service.GetAsync(employee.Id);
    return Results.Accepted();
});

var skillRoutes = app.MapGroup("/api/skills").WithTags("Skills");

skillRoutes.MapPost("", [Authorize(Policy = "SkillManagerPolicy")] async (
    [FromServices] ISkillService service,
    [FromBody] SkillDto dto,
    ClaimsPrincipal user) =>
{
    var userId = GetUserId(user);
    var created = await service.CreateAsync(dto, userId);
    return Results.Created($"/api/skills/{created.Id}", created);
});

skillRoutes.MapGet("", [Authorize] async (
    [FromServices] ISkillService service,
    [FromQuery] Guid? parentSkillId) =>
{
    var skills = await service.ListAsync(parentSkillId);
    return Results.Ok(skills);
});

skillRoutes.MapGet("/{id}", [Authorize] async (
    [FromServices] ISkillService service,
    Guid id) =>
{
    var skill = await service.GetAsync(id);
    return skill is null ? Results.NotFound() : Results.Ok(skill);
});

skillRoutes.MapPut("/{id}", [Authorize(Policy = "SkillManagerPolicy")] async (
    [FromServices] ISkillService service,
    Guid id,
    [FromBody] SkillDto dto,
    ClaimsPrincipal user) =>
{
    var userId = GetUserId(user);
    var updated = await service.UpdateAsync(id, dto, userId);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
});

skillRoutes.MapDelete("/{id}", [Authorize(Policy = "SkillManagerPolicy")] async (
    [FromServices] ISkillService service,
    Guid id,
    ClaimsPrincipal user) =>
{
    var userId = GetUserId(user);
    var deleted = await service.DeleteAsync(id, userId);
    return deleted ? Results.NoContent() : Results.NotFound();
});

skillRoutes.MapGet("/{id}/secondaries", [Authorize] async (
    [FromServices] ISkillService service,
    Guid id) =>
{
    var secondaries = await service.ListSecondariesAsync(id);
    return Results.Ok(secondaries);
});

app.Run();

static string GetUserId(ClaimsPrincipal user) =>
    user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.Identity?.Name ?? "anonymous";
