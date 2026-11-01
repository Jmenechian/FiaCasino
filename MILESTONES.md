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
- Next milestone: milestone 2 below.

## Milestone 2 — Deck and seven-card deal
- Status: complete and user-accepted on 2026-11-01.
- Implemented: complete standard 52-card deck (no jokers), matching artwork in the existing style, Fisher-Yates shuffle followed by a queue, and a Deal button drawing five board cards plus two player cards without replacement.
- Behavior: initial deal on entering Poker; each Deal starts from a fresh shuffled full deck. Cards may repeat between deals. The current deal persists through Settings and lobby navigation during the session. Dealing resets the inspector readout.
- Preserved: existing appearance, card inspector/logging, inspector visibility setting, and navigation.
- Excluded: betting, bots, hand evaluation, staged poker rounds, animations, and customization.
- Technical validation: 1,000 seeded deck/deal checks passed; headless Godot checks passed for 100 deals, all 52 textures, inspector identity/reset, and navigation retaining the deal. C# build passed (NuGet vulnerability-feed warning due to unavailable network); asset import and Windows x64 export passed; exported startup smoke test exited successfully. Final import/export/runtime logs contained no errors or warnings.
- User review: accepted. After receiving the visual/interaction checklist, the user reported "yeah all working perfectly". This records user-reported manual acceptance, not automated visual coverage.
- Next milestone: not scoped or authorized. Discuss the next small increment before implementation.
