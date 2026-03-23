# Foundation Project Usage Guidance

## Building and Running
- Use the `Foundation.sln` solution file from the repository root to build all projects simultaneously.
- Restore NuGet packages with `dotnet restore` and build via `dotnet build` or through Visual Studio.

## Architectural Notes
- Dependency injection is configured through extension methods such as `AddApplicationServices`, simplifying service registration in host projects.
- DTOs (e.g., `EmployeeDto`) are defined within `Foundation.Application`, ensuring a consistent contract for higher layers while keeping domain entities encapsulated.

## Contributor Tips
- Follow the existing naming conventions and structure when adding new entities or services.
- Keep business logic inside the Application or Domain layers and expose functionality via well-defined interfaces to maintain separation of concerns.
