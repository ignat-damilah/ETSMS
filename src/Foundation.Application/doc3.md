# Foundation.Application Documentation

This layer hosts business-logic orchestration for employee data.

## DTOs
- `EmployeeDto` exposes employee identifiers, full name, email, and role, used in API responses.

## Services
- `IEmployeeService` defines methods to list employees and fetch a single employee by ID.
- `EmployeeService` implements `IEmployeeService`, relying on `IEmployeeRepository`. It maps domain entities (`Employee`) to DTOs before returning results.

## Repository Abstraction
- `IEmployeeRepository` declares CRUD-like operations (`GetAllAsync`, `GetByIdAsync`, `AddAsync`).
- `EmployeeService` depends on these operations to fetch domain data.

## Dependency Injection
- `ServiceCollectionExtensions.AddApplicationServices` registers `IEmployeeService` with its concrete implementation.

This separation ensures application logic remains testable, agnostic of persistence details, and easily consumable by the API layer.
