# Milestones

Keep this file concise: agreed scope, implementation status, validation, and user review. Detailed planning belongs in local notes.

## Setup — Project continuity
- Implemented: repository working agreement, publication boundaries, local notes, and session handoff.
- Validation: documentation and Git tracking checks; no application code to test.
- User review: accepted; user authorized proceeding to mockups.

## Milestone 1 — Visual shell and workflow proof
- Status: complete and user-accepted on 2026-11-01.
- Implemented: Poker / Settings / Exit lobby; five board cards and two player cards; per-card inspector and log output; session-only inspector visibility setting; Back/Escape navigation.
- Appearance: user approved proceeding with a simple felt table and readable sample deck rather than selecting a generated concept. Future deck art remains independent of card identity.
- Technical validation: C# build passed with zero warnings/errors; card textures imported; lobby/table visually inspected; Windows x64 export produced; final export and runtime logs contained no error/warning entries.
- User validation: after receiving a checklist for all seven card identities, Settings, Back/Escape, resizing, and Exit, the user reported "yeah all feels good" and accepted the result. This is user-reported manual acceptance, not automated UI coverage.
- Scope limits: fixed sample cards only; no rules, dealing, betting, bots, customization, or Linux verification.
- Publication: source, card SVGs/import settings, Godot configuration, and docs are tracked. Build outputs, tool downloads, concept art, logs, and working notes stay local.
- Next milestone: not scoped or authorized yet. Discuss the next small increment before implementation.
