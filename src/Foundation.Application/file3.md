# Foundation.Application Documentation

This project defines application-level abstractions and orchestrates business logic via services.

## Project Structure
- `DTOs`: Data Transfer Objects shared with API consumers (currently contains `EmployeeDto`).
- `Services`: Service implementations that coordinate repository access and business rules.
- `Interfaces`: Contracts for services and repositories that allow decoupling from infrastructure implementations.
- `Extensions`: `ServiceCollectionExtensions` providing DI registration for the application services.

## DTOs
- `EmployeeDto`: Represents the data transferred to/from the API layer when working with employees (properties mirror the domain entity).

## Services & Interfaces
- `IEmployeeService`: Defines asynchronous operations, such as listing and retrieving employees.
- `EmployeeService`: Implements the application service by depending on `IEmployeeRepository`.

## Dependency Injection
- `ServiceCollectionExtensions.AddApplicationServices`: Registers `IEmployeeService` with its implementation `EmployeeService`.
- The extension method is consumed in the API project to ensure application services are available to the endpoints.

## Collaboration with Infrastructure
- Application services depend on repository interfaces (`IEmployeeRepository`) defined here, enabling the infrastructure project to provide concrete data access implementations without coupling reusable business logic to persistence details.