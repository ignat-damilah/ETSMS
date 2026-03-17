using System;
using System.Linq;
using System.Security.Claims;
using Foundation.API.Authentication;
using Foundation.API.HealthChecks;
using Foundation.API.Middleware;
using Foundation.Application.DTOs;
using Foundation.Application.Services;
using Foundation.Application.Extensions;
using Foundation.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Serilog;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.InstrumentationKey = builder.Configuration["Observability:ApplicationInsights:InstrumentationKey"];
    options.ConnectionString = builder.Configuration["Observability:ApplicationInsights:ConnectionString"];
});

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

var azureAdSettings = builder.Configuration.GetSection("Authentication:AzureAd");

if (!builder.Configuration.GetSection("Security:Cors:AllowWildcard").Get<bool>())
{
    builder.Services.AddCors(options =>
    {
        var origins = builder.Configuration.GetSection("Security:Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        options.AddDefaultPolicy(policy =>
        {
            if (origins.Any())
            {
                policy.WithOrigins(origins)
                      .AllowCredentials();
            }
            else
            {
                policy.AllowAnyOrigin();
            }

            policy.AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });
}
else
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
    });
}

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

builder.Services.AddSingleton<IClaimsTransformation, AzureAdRoleClaimsTransformation>();

builder.Services.AddHttpClient<AzureAdAuthorityHealthCheck>();

var connectionString = builder.Configuration.GetConnectionString("FoundationDatabase") ?? string.Empty;

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "liveness" })
    .AddNpgSql(connectionString, name: "postgresql", tags: new[] { "readiness", "db" })
    .AddCheck<AzureAdAuthorityHealthCheck>("azuread", tags: new[] { "readiness", "auth" });

builder.Services.AddRateLimiter(options =>
{
    var limit = builder.Configuration.GetValue<int?>("Security:RateLimiting:RequestsPerMinute") ?? 120;

    options.GlobalLimiter = PartitionRateLimiter.Create<HttpContext, string>(_ =>
        RateLimitPartition.GetTokenBucketLimiter("global", _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = limit,
            TokensPerPeriod = limit,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            AutoReplenishment = true
        }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddOpenTelemetryTracing(tracing =>
{
    tracing
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Foundation.API"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation(options => options.SetDbStatementForText = true)
        .AddConsoleExporter();
});

builder.Services.AddOpenTelemetryMetrics(metrics =>
{
    metrics
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Foundation.API"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddProcessInstrumentation()
        .AddRuntimeInstrumentation()
        .AddConsoleExporter();
});

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ResponseWrappingMiddleware>();
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpMetrics();
app.UseMetricServer();

app.MapGet("/", () => Results.Ok(new { status = "Foundation API is operational" }));

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("liveness"),
    ResponseWriter = WriteHealthResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("readiness"),
    ResponseWriter = WriteHealthResponse
});

app.MapGet("/employees", [Authorize(Policy = "EmployeePolicy")] async (IEmployeeService service) =>
    Results.Ok(await service.ListAsync()));

app.MapGet("/employees/{id}", [Authorize(Policy = "LeadershipPolicy")] async (IEmployeeService service, Guid id) =>
{
    var employee = await service.GetAsync(id);
    return employee is not null ? Results.Ok(employee) : Results.NotFound();
});

app.MapPost("/employees", [Authorize(Policy = "AdminPolicy")] async (IEmployeeService service, EmployeeCreateDto dto) =>
{
    await service.AddAsync(dto);
    return Results.Accepted();
});

static Task WriteHealthResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";
    var payload = new
    {
        status = report.Status.ToString(),
        checks = report.Entries.Select(entry => new
        {
            name = entry.Key,
            status = entry.Value.Status.ToString(),
            description = entry.Value.Description
        })
    };

    return context.Response.WriteAsJsonAsync(payload);
}

app.Run();
