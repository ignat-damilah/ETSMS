# Foundation.Domain Documentation

The domain project contains core business entities.

## Entities
- `Employee`: Represents an employee within the organization with properties such as `Id`, `FirstName`, `LastName`, `Email`, and `Position`.

## Design Notes
- The domain project is clean and persistence-agnostic, suitable for reuse across application services and infrastructure implementations.
- Entities are kept simple to represent the core concepts and are referenced by both application services and infrastructure repositories.