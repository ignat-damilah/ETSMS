using Foundation.Application.Interfaces;
using Foundation.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<ISkillService, SkillService>();

        return services;
    }
}