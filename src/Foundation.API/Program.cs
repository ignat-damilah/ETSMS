using Foundation.API.Endpoints;
using Foundation.Application.Extensions;
using Foundation.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

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

// OpenAPI / Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Foundation API",
        Version = "v1",
        Description =
            "REST API for the centralised Technology Skill Matrix. " +
            "Provides full CRUD and hierarchy navigation for the Skill Taxonomy (Epic E-02). " +
            "\n\n## Skill Hierarchy Rules\n" +
            "- Skills are organised in a **strict two-level taxonomy**: primary → secondary.\n" +
            "- **Primary skills** have `parentSkillId = null`.\n" +
            "- **Secondary skills** set `parentSkillId` to the ID of an existing primary skill.\n" +
            "- A secondary skill cannot itself be a parent (no tertiary nesting).\n" +
            "- Cyclic and orphaned references are rejected with `400 Bad Request`.\n" +
            "\n## Deletion Constraints\n" +
            "- `DELETE /api/skills/{id}` performs a **soft delete**: the skill record is " +
            "retained in the database with `isActive` set to `false`.\n" +
            "- Soft-deletion is **blocked** (`409 Conflict`) when the skill has one or more " +
            "active (`isActive = true`) secondary children.\n" +
            "- Inactive secondary children do **not** block soft-deletion.\n" +
            "- Soft-deleted skills remain retrievable via `GET /api/skills/{id}` and are " +
            "included in list results when `isActive=false` is passed."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Azure AD JWT bearer token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

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

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Foundation API v1");
    options.RoutePrefix = "swagger";
});

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

// Skill endpoints
app.MapSkillEndpoints();

app.Run();
