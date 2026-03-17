# Foundation.Infrastructure Documentation

This project implements persistence concerns and provides EF Core wiring.

## Project Contents
- `Data/FoundationDbContext`: EF Core `DbContext` exposing a `DbSet<Employee> Employees`. It configures the database context and manages database access for repositories.
- `Repositories/EmployeeRepository`: Implements `IEmployeeRepository`, interacts with `FoundationDbContext` to perform CRUD operations on employee entities.
- `Extensions/ServiceCollectionExtensions`: Registers infrastructure services, including `FoundationDbContext` and the concrete repositories, into the DI container.

## Relationship to Other Layers
- Infrastructure depends on the domain project for entity definitions but remains isolated from higher layers.
- The API layer uses infrastructure services through interfaces defined in the application project, maintaining a clean separation via DI extensions.

## Notes
- Infrastructure services are added to the API project via `Foundation.Infrastructure.Extensions.ServiceCollectionExtensions.AddInfrastructureServices`.
- Any database connection strings or providers should be configured in the consuming application's configuration (e.g., `appsettings.json`).