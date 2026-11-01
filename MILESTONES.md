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
- Next milestone: best-hand display, scoped below.

## Milestone 3 — Best-hand display
- Status: implemented and technically verified on 2026-11-01; awaiting user review.
- Scope: evaluate the best five-card hand from the seven displayed cards and show a concise hand-category label, including royal flush where applicable. No poker tutorial required.
- Validate: hand categories, tie-break comparisons, ace-low straights, and best-five selection; preserve Deal, inspector, navigation and existing appearance.
- Excluded: odds, opponents, betting, staged rounds, bots, and artwork customization.
- Implemented: independent C# evaluator enumerates five-card subsets, retains comparison ranks and best-five identities, and displays BEST HAND in the existing footer. Royal flush is a display label for an ace-high straight flush. The label updates on Deal and survives inspector visibility/navigation changes.
- Technical validation: exhaustive category census of all 2,598,960 five-card hands passed, plus targeted category ordering, kickers, suit-neutral ties, ace-low straights, seven-card selection and invalid-input checks. Existing 1,000 seeded deck checks passed. App build passed with zero warnings/errors. Running Godot probe independently checked category labels across 100 deals, all 52 textures, inspector identity/reset/visibility, and navigation retaining cards/label. Rendered table inspected at 960x600 and 1120x700 with inspector shown/hidden; no clipping seen.
- Build delivery: Godot import and Windows x64 export completed successfully (export exit 0); exported headless startup smoke check exited 0. Final import/scene/export/smoke logs contain no errors or warnings. Release validation covers startup; full interaction checks ran against the development scene.
- User review: pending. Review the updated Windows build; check label accuracy across deals, inspector independence, navigation, and resizing. Do not start milestone 4 until accepted and authorized.
- Next action: user manual review and feedback; then record acceptance or fix reported issues.
- Broader direction: enjoyable offline Hold'em first, one opponent initially, fair bots, Free Play with refills and later tournaments without refills. Mobile deferred; appearance choices optional. Detailed future planning stays in local notes.
