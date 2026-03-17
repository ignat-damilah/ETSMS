# Foundation.Domain Documentation

This project defines the domain entities and invariants for the Foundation service.

## Entity Definitions
- `Employee` is the primary entity, containing fields for `Id`, `FirstName`, `LastName`, `Email`, `Role`, and `CreatedAt`.
- `FullName` is a derived property that concatenates the first and last names.

## Domain Characteristics
- All string properties default to empty strings to avoid nullability.
- `CreatedAt` is initialized to `DateTime.UtcNow` and represents the entity creation timestamp.
- Domain logic is intentionally minimal; responsibilities such as validation and persistence live in the application and infrastructure layers.
