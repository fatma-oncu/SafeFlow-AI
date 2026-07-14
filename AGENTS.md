# AGENTS.md — AI Agent Behavior & Governance Rules

This document defines behavioral constraints, interaction styles, and tool usage protocols for AI agents (including coding assistants) working on the SafeFlow-AI codebase.

---

## 1. General Principles

*   **No Code Without Approval:** Never generate, modify, or insert application code (C#, Dart/Flutter, configuration files) unless the user has explicitly requested or approved it.
*   **Documentation Alignment:** Before writing any code, confirm that the proposed design is in complete agreement with all governing documents (`ARCHITECTURE_RULES.md`, `SECURITY_GUIDELINES.md`, `CODING_STANDARD.md`, and the `docs/` folder).
*   **Non-Destructive Modifications:** Always preserve existing comments, docstrings, and unrelated methods in target files unless specifically asked to refactor or delete them.

---

## 2. Communication Style

*   **Conciseness:** Keep responses structured, concise, and focused. Avoid verbose conversational filler.
*   **Symbol and File Links:** Always create clickable links for files and code symbols (classes, methods, structs) in your responses using GitHub-style markdown syntax:
    *   *Correct:* `[UserRepository](file:///c:/Users/drmus/Desktop/SafeFlow-AI/src/SafeFlow.Infrastructure/Repositories/UserRepository.cs#L20-L45)`
    *   *Incorrect:* `\`UserRepository.cs\``
*   **Clarification:** If a requirements description is ambiguous or conflicts with architectural rules, pause and ask the user for clarification instead of guessing or making assumptions.

---

## 3. Tool Usage Constraints

*   **No Redundant Searches:** Do not run repetitive or broad searches over the codebase. First, read `CLAUDE.md` and check the file map. Use exact path lookups via `view_file` or precise searches via `grep_search`.
*   **No Sandboxed Command Loops:** Do not execute scripts or tests in loops. Run commands once, read the output, address issues, and then run again.
*   **No Write Wildcards:** Never modify entire files or overwrite files if partial edits (`replace_file_content` or `multi_replace_file_content`) are sufficient.
*   **Target Directory Boundaries:** Do not write temporary or build files outside the workspace root (`c:\Users\drmus\Desktop\SafeFlow-AI\`). Do not write files directly into system paths (e.g., AppData, Temp) unless specifically instructed.

---

## 4. Slash Commands and User Guides

Suggest slash commands when appropriate to help guide the user's workflow:
*   `/goal`: Use when the user wants to execute a long, multi-stage task thoroughly and autonomously.
*   `/schedule`: Use when a task requires recurring runs, health monitoring, or deferred tasks.
*   `/grill-me`: Use when align-on-plan is required to resolve structural design decisions.
*   `/learn`: Use when a correction has been made, or a complex workspace configuration needs to be persisted for future turns.
