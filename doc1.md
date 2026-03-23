# Foundation Project Overview

The Foundation solution is a multi-layered .NET application structured into API, Application, Domain, and Infrastructure projects.

- **Foundation.Domain**: Defines core entities such as the `Employee` class, capturing identity, contact information, and role metadata. Derived properties like `FullName` provide convenience accessors for consuming layers.
- **Foundation.Application**: Hosts DTOs and service abstractions; for example, `EmployeeDto` models shared data contracts and `ServiceCollectionExtensions` wires up application services via dependency injection.
- **Foundation.API / Infrastructure**: Serve as the entry point and supporting infrastructure for hosting and persisting the domain model (details documented elsewhere).

This repository is organized to support clean separation of concerns, with each project focusing on a specific responsibility within the overall system architecture.