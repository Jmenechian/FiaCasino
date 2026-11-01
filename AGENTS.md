# Project working agreement

## Start every session
1. Read this file, `README.md`, and `MILESTONES.md`.
2. If present, read `local-notes/Start Here.md`, `local-notes/Current Work.md`, and notes linked from the current task.
3. Inspect Git status and relevant code before editing. Preserve unrelated changes.
4. Identify the current milestone, its review status, and the next authorized step. Do not assume a new chat or model has previous conversation context.

Local notes are deliberately ignored by Git. On a fresh clone they will be absent: use the tracked files for established constraints, and ask for missing context only when it changes the next action. Never invent prior approvals. These instructions are model-independent; no specific model or delegation is required.

## Product constraints
- One personal, free-to-play, offline application containing multiple games. No real-money payments, accounts, servers, or online multiplayer planned.
- Windows first; Linux is desirable where inexpensive to support. A basic run guide is enough initially.
- Godot + C# is the agreed initial direction, subject to an early practical workflow check. Do not claim the engine workflow is proven until it has been built, run, visually inspected, and revised.
- Start with a button-based lobby and optional background. A walkable casino is a distant possibility, not current scope.
- Preserve room for independently selectable backgrounds, table surfaces, card faces/backs, and effects. Do not build customization, a theme editor, or multiple styles merely because they might be wanted later.
- Keep game logic separate from presentation where useful. Share abstractions only when actual needs justify them; no speculative universal game framework or plugin loader.
- Use free tools/assets initially and record required asset attribution/licenses. Branding is provisional.

## Collaboration and review
- The assistant owns implementation, validation, and notes. Write documentation in English; user feedback may be English or Spanish.
- Work in small, independently reviewable milestones. Complete the agreed scope, explain how to inspect it, then pause for user review before the next milestone.
- Use mockups before implementing a new visual direction. Appearance is a priority; do not silently redesign it.
- Discuss significant architecture changes before implementation. Make routine implementation choices independently within the approved scope.
- Do not repeatedly ask settled questions. Ask when an unresolved preference materially affects the result or a review decision is required.
- Explain interesting systems in local notes: how they work, why this approach, limitations, and concrete examples. Bot reasoning is of particular interest when bots are actually implemented.
- Run checks appropriate to the change. For UI work, inspect the rendered application as well as build output. Report unverified behavior honestly.

## Documentation and handoff
- Keep GitHub lean: source, assets/licenses, setup, README, milestone records, this working agreement, and necessary configuration.
- Keep brainstorming, future plans, detailed tutorials, and working task notes in `local-notes/` (an Obsidian-compatible Markdown vault). Never publish chat transcripts.
- Maintain meaningful note links for Obsidian's graph. Use explicit diagrams for software structure; the note graph is not an automatically generated dependency graph.
- Before ending a work session, update `local-notes/Current Work.md` with what changed, checks/results, blockers, review status, and the exact next step. Update `MILESTONES.md` when milestone status changes.
- Distinguish proposed, implemented, verified, and user-accepted work. Keep documentation consistent with the actual repository.
- Local notes are not backed up to GitHub. Do not imply they travel with a fresh clone.

## Git
- The user authorizes commits and pushes; direct commits to `main` are acceptable. PRs are optional.
- Review staged paths and diffs before committing. Never stage local planning notes, secrets, editor caches, or build outputs.
- Commit coherent changes. A commit/push is not evidence of user acceptance; keep review status explicit.
- At the first milestone review, show what is tracked versus kept local so the user can refine this boundary.
