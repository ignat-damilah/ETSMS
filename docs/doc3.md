# Application, Domain, and Infrastructure Layers

## Application Layer

- `Foundation.Application.DTOs.EmployeeDto` represents the data transferred from services to API consumers with `Id`, `FullName`, `Email`, and `Role`.
- `Foundation.Application.Interfaces.IEmployeeRepository` defines the repository contract for employee persistence: `GetAllAsync`, `GetByIdAsync`, and `AddAsync`.
- `Foundation.Application.Services.IEmployeeService` exposes two operations: `ListAsync` and `GetAsync`, returning DTOs.
- `Foundation.Application.Services.EmployeeService` implements the service interface by relying on `IEmployeeRepository`.
- `Foundation.Application.Extensions.ServiceCollectionExtensions` registers the application service into the dependency injection container.

## Domain Layer

- `Foundation.Domain.Entities.Employee` is the aggregate root that models employees with first/last names, email, role, creation timestamp, and a computed `FullName` property.

## Infrastructure Layer

- `Foundation.Infrastructure.Data.FoundationDbContext` extends `DbContext` to expose `DbSet<Employee>` and configure entity mappings (email max length, required, etc.).
- `Foundation.Infrastructure.Repositories.EmployeeRepository` implements `IEmployeeRepository` using EF Core, providing ordered lists, lookups by ID, and asynchronous additions.
- `Foundation.Infrastructure.Extensions.ServiceCollectionExtensions` registers the `FoundationDbContext` and repository. The DbContext uses a PostgreSQL connection string named `FoundationDatabase`.

By respecting these layers, the project keeps UI/API concerns separate from business logic and persistence. DI extension methods in the application and infrastructure projects make wiring up services to `Foundation.API` clean and declarative.
