using Foundation.Application.Interfaces;
using Foundation.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FoundationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("FoundationDatabase")
                ?? throw new InvalidOperationException("Connection string 'FoundationDatabase' is not configured.");

            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<ISkillRepository, SkillRepository>();

        return services;
    }
}
