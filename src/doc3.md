# Domain Layer Overview

The domain layer models the `Employee` entity used across the solution:

- **Properties**:
  - `Id`: Primary identifier of type `Guid`.
  - `FirstName` / `LastName`: Mutable strings that combine into `FullName`.
  - `Email`: Contact address stored as a required field.
  - `Role`: Optional string indicating the employee role.
  - `CreatedAt`: Tracks entity creation with UTC timing.
- **Computed Property**:
  - `FullName` concatenates first and last names with a space, ensuring consistent display.

This layer is intentionally simple, providing a single enterprise entity and keeping behavior close to its data representation.
