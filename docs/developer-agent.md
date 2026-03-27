# Developer Agent Documentation

## Overview
This developer agent is part of the CI/CD automation for the Foundation project. It serves as an extension of the development team, capable of reading requirements, exploring the repository, and implementing requested changes. When you "write docs with the developer agent," provide guidance to the agent through the requirements doc or PR description, and it will update the repository accordingly.

## Interaction Guidelines
1. **Clear Requirements**: Describe exactly what documentation or code changes you need. Mention any specific file names, sections, or formatting expectations.
2. **Scope**: Keep requests focused. The agent will perform minimal, precise updates based on the requirement, so avoid combining unrelated tasks in one request.
3. **Feedback**: If revisions are needed after the agent completes a task, submit a follow-up requirement to adjust or extend the documents.

## Example Workflow
1. Create a requirements document that spells out the documentation deliverable you want (e.g., a new developer onboarding guide). 
2. The agent explores the repository, understands conventions, and drafts a plan using `submit_plan` before modifying files.
3. The agent updates or adds documentation files (such as this one), following your style and content guidance.
4. After changes are implemented, the agent creates a pull request so you can review and merge the updates.

## Additional Notes
- The developer agent respects existing code styles and strives to maintain clean, concise wording in documentation.
- For iterative collaboration, you can pair this agent with other contributors by providing detailed review feedback or new requirements.
