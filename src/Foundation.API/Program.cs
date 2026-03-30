using Foundation.Application.Extensions;
using Foundation.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Observability
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["Observability:ApplicationInsights:ConnectionString"];
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

app.MapGet("/employees", [Authorize(Policy = "EmployeePolicy")] async ([FromServices] Foundation.Application.Services.IEmployeeService service) =>
    Results.Ok(await service.ListAsync()));

app.MapGet("/employees/{id}", [Authorize(Policy = "LeadershipPolicy")] async ([FromServices] Foundation.Application.Services.IEmployeeService service, Guid id) =>
{
    var employee = await service.GetAsync(id);
    return employee is not null ? Results.Ok(employee) : Results.NotFound();
});

app.MapPost("/employees", [Authorize(Policy = "AdminPolicy")] async ([FromServices] Foundation.Application.Services.IEmployeeService service, [FromBody] Foundation.Domain.Entities.Employee employee) =>
{
    await service.ListAsync();
    return Results.Accepted();
});

app.Run();
