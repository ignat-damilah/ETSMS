# Foundation Solution Overview

This repository hosts the **Foundation** solution, a layered backend designed to provide a secure employee management API. The solution is divided into discrete projects with clear separation of responsibilities:

- **Foundation.API**: ASP.NET Core minimal API exposing endpoints, middleware, authentication/authorization, observability, and health checks. This layer wires together the downstream services and infrastructure dependencies.
- **Foundation.Application**: Application services, DTOs, interfaces, and dependency injection helpers that encapsulate business workflows. It represents the application boundary and orchestrates use cases.
- **Foundation.Domain**: Domain entities that model the core business concepts (for example, `Employee`). This project is intended to be persistence-agnostic.
- **Foundation.Infrastructure**: Persistence implementation, EF Core `DbContext`, repositories, and service collection extension helpers. This layer depends on the domain abstractions to provide concrete data access.

Each layer is referenced appropriately from higher layers, ensuring a clear flow from API consumers through application logic to infrastructure details. Documentation for each project is provided within its folder to give further insight into endpoints, services, and implementation notes.