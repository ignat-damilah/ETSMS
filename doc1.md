# Project Documentation Overview

This repository hosts the Foundation service, composed of several projects aligned with clean architecture principles. Each layer has a focused responsibility:

- **Foundation.API** exposes HTTP endpoints, middleware, and observability configurations.
- **Foundation.Application** contains DTOs, service contracts, and business logic orchestrating domain entities and repositories.
- **Foundation.Domain** defines the core entities and invariants of the foundation domain model.
- **Foundation.Infrastructure** implements data persistence, repositories, and infrastructure-specific service registration.

Subsequent documents within each project directory provide deeper explanations tailored to that layer.
