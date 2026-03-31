using Foundation.Application.Interfaces;
using Foundation.Infrastructure.Data;
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

            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();

        return services;
    }
}
