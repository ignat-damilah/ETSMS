# Skill Taxonomy API

## Overview
The Skill Taxonomy API exposes RESTful endpoints to manage primary and secondary skills, enforce hierarchy rules, support soft deletion, and provide filtering/hierarchy navigation surfaces for clients.

## Endpoints
### Create Skill
`POST /skills`
- Body: `SkillDto` (Id optional)
- Enforces that secondary skills must supply `ParentSkillId` and primary skills cannot have one.
- Returns `201 Created` with the created skill.

### Retrieve Skill
`GET /skills/{id}`
- Query: `includeDeleted` (bool, optional)
- Returns `200 OK` with skill details if found, otherwise `404`.

### Update Skill
`PUT /skills/{id}`
- Body: `SkillDto` (must match validation rules)
- Enforces hierarchy integrity and updates `IsActive`/`IsDeleted` flags.
- Returns `200 OK` with updated skill.

### Soft Delete Skill
`DELETE /skills/{id}`
- Sets `IsDeleted = true`.
- Primary skills with children cannot be deleted and return a validation error.
- Returns `204 No Content` on success.

### List/Filter Skills
`GET /skills`
- Query Parameters (all optional):
  - `isActive` (bool)
  - `category` (`Primary` or `Secondary`)
  - `parentSkillId` (`Guid`)
  - `includeDeleted` (bool)
  - `page` (int, defaults to `1`)
  - `pageSize` (int, defaults to `50`)
- Default response excludes soft-deleted records and returns a paged list.

### Hierarchy
- `GET /skills/{id}/parent`: Returns the parent skill if it exists (404 when missing).
- `GET /skills/{id}/children`: Returns immediate active children (excluding deleted records).

## Filtering Usage
- Combine `isActive`, `category`, and `parentSkillId` to refine searches.
- Use `includeDeleted=true` only when you must examine soft-deleted entries.
- Pagination parameters (`page`, `pageSize`) are applied after filtering to control payload size.

## Integrity Rules
- Secondary skills require a valid parent skill.
- Primary skills cannot have a parent.
- Soft deleting a primary skill is blocked if any non-deleted secondary skills exist.

## DTO Shape (`SkillDto`)
```json
{
  "id": "string",
  "name": "string",
  "category": "Primary|Secondary",
  "parentSkillId": "string|null",
  "isActive": true,
  "isDeleted": false
}
```
