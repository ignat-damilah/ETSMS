# Foundation.Infrastructure Documentation

This project implements persistence concerns and integration with external infrastructure APIs.

## Database Context
- `FoundationDbContext` derives from `DbContext` and exposes a `DbSet<Employee>`.
- Configures the `Employee` entity with required email, max-length for email and role, and primary key on `Id`.

## Repositories
- `EmployeeRepository` implements `IEmployeeRepository`.
  - `GetAllAsync` retrieves all employees, orders them by last and first name, and executes as no-tracking queries.
  - `GetByIdAsync` fetches a single employee by identifier using no-tracking semantics.
  - `AddAsync` adds a new employee and saves the changes within the DbContext.

## Dependency Injection Extensions
- `ServiceCollectionExtensions.AddInfrastructureServices` reads the `FoundationDatabase` connection string and registers the DbContext with PostgreSQL.
- Registers `IEmployeeRepository` with the concrete `EmployeeRepository`.

This layer isolates EF Core configuration and database access, keeping other projects dependent on abstractions.
