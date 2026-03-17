# Application Layer Overview

This layer orchestrates domain entities and infrastructure repositories to expose application-specific services and DTOs.

- **DTOs**: `EmployeeDto` is the sanitized projection of the `Employee` entity, exposing `Id`, `FullName`, `Email`, and `Role`.
- **Services**: `EmployeeService` implements `IEmployeeService`, mapping domain data into DTOs:
  - `ListAsync` returns all employees ordered by name.
  - `GetAsync` retrieves a single employee by identifier.
  - Both methods rely on `IEmployeeRepository` to access persistence.
- **Dependency Injection**: `ServiceCollectionExtensions` registers `IEmployeeService` with its implementation so the API layer can depend on the IServiceCollection extensions.
- **Repository Contract**: `IEmployeeRepository` abstracts data access, exposing methods to list, fetch by id, and add employees, each accepting a `CancellationToken`.

This layer keeps business logic cohesive by defining DTO transformations and service boundaries.
