# CLAUDE Guidance

## Repository Overview
- Solution: `Foundation.sln`
- Projects are located under `src/`:
  - `Foundation.API/` - Web/API layer
  - `Foundation.Application/` - Application services and CQRS patterns
  - `Foundation.Domain/` - Core domain logic, entities, and value objects
  - `Foundation.Infrastructure/` - Infrastructure, persistence, integrations

## Conventions
- Follow existing C# coding styles (PascalCase types/methods, camelCase parameters)
- Keep files organized by project responsibilities
- Prefer dependency injection, asynchronous methods, and clean architecture patterns already in place

## Common Commands
- `dotnet build Foundation.sln`
- `dotnet test Foundation.sln`
- `dotnet format` (if formatting required)

## Agent Notes
- Focus on minimal, targeted changes
- Respect existing project structure and dependencies
- Indicate owners or reviewers if requested
