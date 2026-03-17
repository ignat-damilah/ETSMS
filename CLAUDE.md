# CLAUDE File

## Repository Overview
- This repository contains a multi-project solution named `Foundation` organized into API, Application, Domain, and Infrastructure layers under the `src/` directory.
- The root solution file (`Foundation.sln`) orchestrates the build across these layered projects.

## Projects
- `Foundation.API/`: Hosts the API layer (likely ASP.NET Core) serving as the entry point for HTTP requests.
- `Foundation.Application/`: Contains application services, CQRS handlers, or business workflows consumed by the API layer.
- `Foundation.Domain/`: Encapsulates domain entities, value objects, and domain services for the core business logic.
- `Foundation.Infrastructure/`: Implements persistence, integration, and other infrastructure concerns used by the higher layers.

## Building and Testing
- Use the provided `Foundation.sln` to build and run the projects. Typical commands include `dotnet build Foundation.sln` and individual project tests or tools as needed.
- Adjust configuration as required for environment-specific settings (seek guidance from project documentation when available).

## Adding Contributions
- Follow existing layering conventions when adding new functionality: update the domain, expose services through the application layer, and wire them into the API via dependency injection.
- Infrastructure changes should be consistent with existing implementations to ensure seamless operations.

## Notes
- No other documentation was included with the repository; maintainers may add more targeted guidance in the future.
