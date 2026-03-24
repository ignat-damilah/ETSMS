# Skill Entity Taxonomy Documentation

## Schema Decisions
- Each skill is represented with `Id`, `Name`, `Category`, `ParentSkillId`, and `IsActive`. `Name` and `Category` are required strings with sensible length constraints.
- Skills can reference another skill through `ParentSkillId` to form a hierarchy. The relationship is configured as a self-referencing one-to-many.
- Skills default to active (`IsActive = true`) when created, aligning with the operational expectation.

## Validation Rules
- `ParentSkillId`, when provided, must reference an existing skill to maintain referential integrity.
- Circular relationships are detected and rejected by checking if the target parent is already a descendant of the modified skill.
- Creating or updating with invalid parent references throws descriptive validation errors.

## Deletion Constraints
- Skills are soft deleted by toggling `IsActive` to `false`, allowing historical relationships and references to remain intact while preventing them from appearing in active listings.
- A skill cannot be soft deleted while it has active children. The service enforces this constraint and returns a clear message advising deactivation of dependents before retrying.
- Dependency checks query for active children before issuing soft deletes to ensure consistent hierarchy state.

## Hierarchy Behavior
- The repository supplies efficient ancestor and descendant traversal by loading skills once per request and operating on in-memory collections keyed by `Id`.
- Ancestor traversal walks upward using dictionary lookups; descendants are discovered via breadth-first search using a queue keyed by parent relationships.
- Hierarchy endpoints can filter ancestors and descendants by activity state to support active-only or full views.

## Testing Strategy
- Unit/integration tests target the `SkillService` to cover CRUD behaviors, hierarchy retrieval, and deletion validations.
- An in-memory EF Core context keeps tests isolated and fast while exercising real repository logic.
- Critical paths include preventing circular dependencies, respecting active-child deletion guards, and ensuring hierarchy responses include expected descendants.

## Performance & Deployment Considerations
- The in-memory traversal approach scales for typical taxonomy sizes, minimizing repeated database calls when assembling hierarchies.
- Real deployments should ensure PostgreSQL indexes exist on `ParentSkillId` for fast child lookups. The model definition includes the necessary index.
- Auditing and deletion messages can integrate with monitoring tools via observability pipelines already configured in the API.
