# Developer Agent Documentation

This document provides guidance on collaborating with the Developer Agent, outlining its role, expectations, and how to write documentation articles with the agent's assistance.

## Purpose
The Developer Agent is an automated collaborator in a CI/CD pipeline. It helps you write documentation by:

- Understanding the repository structure and existing conventions.
- Following the product owner's requirements faithfully.
- Adding or updating documentation files based on project needs.

## Working with the Developer Agent
1. **Clarify requirements** – Start with a clear brief or requirements document, describing what documentation is needed.
2. **Explore only what?s necessary** – The agent lists the root structure, then zooms into folders relevant to the requested changes.
3. **Plan before coding** – The agent writes a short implementation plan outlining the files to change.
4. **Implement cleanly** – Write or update documents/modules using existing styles and naming conventions.
5. **Submit a PR** – After finishing, the agent summarizes the changes and opens a pull request.

## Writing Docs with the Developer Agent
- **Keep it concise** – The agent favors focused documentation that explains purpose, usage, and expectations.
- **Follow existing patterns** – If other docs exist, follow their structure. (In this repo, there were no docs, so a new `docs/` folder was introduced.)
- **Provide context** – Include instructions for future contributors on how to maintain or extend the documentation.

## Example: Adding a new doc
1. Describe the feature or guide you want to document.
2. Have the agent analyze the repo structure to determine where the doc belongs.
3. The agent drafts the content, saving it under `docs/` or another appropriate directory.
4. Review the doc and update it with additional context if needed.

## Next Steps
- Review this doc for accuracy and clarity.
- Update any additional guidelines for writing docs with the agent if processes change.
