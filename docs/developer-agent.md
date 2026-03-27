# Developer Agent Documentation

## Overview
This document summarizes how to engage with the developer agent to produce documentation within this repository. It clarifies expectations, outlines available inputs, and highlights best practices for consistent collaboration.

## Workflow Expectations
1. **Request and Clarify**: Provide a well-defined task or documentation requirement. Include context such as target audience, required level of detail, and any relevant constraints (e.g., styling guides, format). 
2. **Agent Exploration**: The agent will inspect the repository structure and relevant files using `list_files` and `read_file` commands to understand existing conventions.
3. **Planning**: A high-level plan is submitted before changes are made. Expect a concise description of the modifications, including the files to be touched.
4. **Implementation**: The agent generates documentation content based on the request. It will follow established conventions and produce clean, professional writing.
5. **Review & PR**: After changes are finalized, a pull request is proposed describing the documentation updates. Once approved, the documentation is merged.

## Inputs the Agent Requires
- **Purpose**: Clarify why the documentation is needed (new feature, onboarding, compliance, etc.).
- **Audience**: Indicate who will consume the documentation (developers, stakeholders, operators, etc.).
- **Format Requirements**: Mention formatting needs such as markdown structure, code snippets, diagrams, or recommended length.
- **Scope**: Specify whether the doc should cover processes, APIs, architectural diagrams, onboarding steps, or other areas.

## Best Practices
- **Be Specific**: Detailed inputs lead to more accurate outputs. Share examples, expected tone, and sections to include.
- **Use Existing Patterns**: Align with repository conventions—namespaces, structure, and documentation styles.
- **Ask for Iterations**: If a section is unclear or needs refinement, request iterative updates.
- **Cite References**: When referencing repository code or architecture, mention key files or modules to consult.
- **Request Reviews**: Have at least one reviewer confirm the documentation for accuracy and clarity.

## Maintenance Notes
- Keep the documentation up to date with workflow changes; revisit this doc whenever the developer-agent collaboration process evolves.
- Encourage contributors to reference existing docs to avoid duplication.
