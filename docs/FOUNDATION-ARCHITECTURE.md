# Foundation Architecture

## Solution Structure
| Project | Responsibility |
|---------|----------------|
| **Foundation.Domain** | Defines the immutable/business entities that represent core concepts (e.g., `Employee`). This layer has no dependencies on other solution layers and can be referenced from both infrastructure and application layers. |
| **Foundation.Application** | Houses service interfaces, DTOs, and business orchestration logic. Application services interact with repositories defined by infrastructure interfaces and return DTOs shaped for API consumers. |
| **Foundation.Infrastructure** | Implements persistence (Entity Framework Core + PostgreSQL) and other integrations. It registers `FoundationDbContext`, repository implementations, and exposes extension methods for `IServiceCollection`. |
| **Foundation.API** | Provides the HTTP surface area via ASP.NET Core minimal APIs. It wires authentication, authorization, CORS, observability, rate limiting, and depends on application services for behavior. |

## Layer Responsibilities
### Foundation.Domain
- Contains entities used across the solution (`Employee` with fields such as `Id`, `FirstName`, `LastName`, `Email`, `Role`, `CreatedAt`, and computed `FullName`).
- `Employee` entity validates data boundaries through EF Core configuration (e.g., required email, max lengths).

### Foundation.Application
- Defines `IEmployeeService` which exposes read operations returning `EmployeeDto` instances.
- Implements `EmployeeService` that orchestrates `IEmployeeRepository` to load data and map to DTOs.
- Provides DTO definitions in `DTOs/EmployeeDto.cs` for API responses.

### Foundation.Infrastructure
- Configures `FoundationDbContext` with `Employees` DbSet and fluent API rules.
- Provides extension `AddInfrastructureServices` to register EF Core, repositories, and connection strings.
- Implements the `EmployeeRepository` that fulfills `IEmployeeRepository` by using `FoundationDbContext` for data access.

### Foundation.API
- Registers services from Application and Infrastructure via extension methods (`AddInfrastructureServices`, `AddApplicationServices`).
- Applies JWT Bearer authentication with Azure AD and defines role-based policies.
- Configures CORS, rate limiting, Application Insights, and health checks.
- Exposes minimal API endpoints for `/employees` plus health/metrics endpoints.

## Observability & Infrastructure
- **Health Checks**: `/health/live`, `/health/ready`, and `/health` endpoints with custom JSON responses.
- **Metrics**: Placeholder endpoint returning `Metrics endpoint`. Replace with real instrumentation (e.g., Prometheus) if needed.
- **Application Insights**: Instrumentation key and connection string sourced from configuration.
- **Rate Limiting**: Token bucket limiter globally applied for 100 requests per minute.

## Security
- Azure AD JWT Bearer authentication ensures tokens are validated against configured Authority and Audience.
- Custom policies (`EmployeePolicy`, `LeadershipPolicy`, `AdminPolicy`) gate `/employees` endpoints by role.
- Role claims use `System.Security.Claims.ClaimTypes.Role`. Ensure tokens include proper role claims.

## Extending the System
- **Repository Layer**: Add interfaces to `Foundation.Application.Interfaces` and implement them in `Foundation.Infrastructure.Repositories`.
- **Application Services**: Register new services through extension methods (not shown but consistent with current patterns).
- **API Layer**: Add endpoint definitions directly in `Program.cs` or extract to dedicated endpoint classes within `Foundation.API.Endpoints`.

## Running and Deploying
1. Ensure PostgreSQL and configuration settings are available.
2. Run EF Core migrations if they exist (not included). Create the database manually using the `FoundationDbContext` model definitions.
3. Deploy the API project to any ASP.NET Core compatible host.
4. Secure the API with Azure AD by providing a valid audience and authority, mapping roles for access control.
