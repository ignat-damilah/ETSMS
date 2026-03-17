# Foundation Solution Overview

This repository contains the **Foundation** solution, composed of API, application, domain, and infrastructure projects. It employs layered architecture principles to isolate concerns:

- `Foundation.API` exposes HTTP endpoints, configures authentication/authorization, observability, rate limiting, and CORS policies.
- `Foundation.Application` defines DTOs, service interfaces, and service implementations that orchestrate business operations without direct infrastructure dependencies.
- `Foundation.Domain` owns the core entity definitions (such as `Employee`) and business invariants that are shared across layers.
- `Foundation.Infrastructure` provides persistence through Entity Framework Core, implements repositories, and extensibility methods to wire up services.

Configuration is driven via JSON files (e.g., `appsettings.json` for the API) and standard host/ASP.NET Core configuration providers. Dependency injection is configured through extension methods that register services and DbContext instances.

Documentation files in this folder describe each layer and its role. Refer to `doc2` for API-specific details and `doc3` for the rest of the backend services.