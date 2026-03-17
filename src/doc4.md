# Infrastructure Layer Overview

This layer is responsible for persistence, database configuration, and wiring repository dependencies.

- **Data Access**:
  - `FoundationDbContext` defines the `Employees` `DbSet` and configures the entity (primary key, email length, role length).
- **Repositories**:
  - `EmployeeRepository` implements `IEmployeeRepository`, providing helpers to list, fetch, and add employees.
  - Queries use `AsNoTracking` and ordered materialization to protect state tracking.
- **Dependency Registration**:
  - `ServiceCollectionExtensions.AddInfrastructureServices` configures the PostgreSQL context using the `FoundationDatabase` connection string (throws if missing) and registers the repository.

Infrastructure services are injected into the Application layer so that API endpoints remain focused on HTTP concerns.
