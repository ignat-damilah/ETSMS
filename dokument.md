# Foundation Project Documentation

This document provides an overview of the Foundation application's dependency injection setup using the provided extension in the Application project.

---

## ServiceCollectionExtensions

The `ServiceCollectionExtensions` class in the Application layer defines an extension method to register all application-level services with the dependency injection (DI) container.

```csharp
// File: src/Foundation.Application/Extensions/ServiceCollectionExtensions.cs
using Foundation.Application.Interfaces;
using Foundation.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeService, EmployeeService>();
        return services;
    }
}
```

### Registered Services

| Interface           | Implementation    | Lifetime  |
|---------------------|-------------------|-----------|
| IEmployeeService    | EmployeeService   | Scoped    |

---

## How to Use

1. In your API project (e.g., in `Program.cs`), import the namespace:
   ```csharp
   using Foundation.Application.Extensions;
   ```
2. Call the extension method when configuring services:
   ```csharp
   var builder = WebApplication.CreateBuilder(args);
   
   // Register application services
   builder.Services.AddApplicationServices();
   
   var app = builder.Build();
   ```

This ensures that all application services (such as the `IEmployeeService`) are available via DI throughout the project.

---

*Generated documentation focusing on the Application project extensions.*
