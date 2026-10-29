# Branch C-2: `base + refs → Powers` (Focused Power Emblem Route)

Read the requested power's effect/stack behavior, motif brief, mapping and status, ARTS.md 5.4, [the shared workflow](../workflow.md), and the active [engine section](../engines.md). A supplied motif uses [reference roles and landmarks](../references.md#1-d-rough-sketch-or-reference-image--finished-asset-refs-or-draftsguidepng). Use the recipe below and [observation tools](../observation-tools.md); unrelated character body and costume sections are needed only when depicted.

1. **Establish scope and motif references:** read power definitions and stack behavior in `<char>.md`, `<char>-art.md`, and `<char>-assets.md`. Open the official status anchor ([status-icons-in-combat-crop.png](../../../../docs/research/sts2-style-refs/future-assets/status-icons-in-combat-crop.png); [study §2](../../../../docs/research/sts2-future-asset-reference-study.md#2-relic-and-status-information-density)). When user-supplied symbol roughs or motif PNGs exist (such as `docs/characters/<char>/refs/powers/`), open each file directly—note that dark/black symbols on transparent backgrounds must be inspected on a light board—and preserve the user's chosen motif rather than reinterpreting from the power title alone. Apply any documented user overrides in `<char>-art.md`.
2. **Generate symbolic state emblems (Recipe 6 & 1-D):** pass the motif rough/reference and official status anchor directly to generate a `256 × 256` RGBA single persistent-state emblem (2–3 bold interlocking shapes, 2–3 primary colors, no miniature card scene or box frame). Place the primary identifying feature in `x/y:15–80%` (central to upper-left) and keep the bottom-right `x/y:60–100%` stack-number zone quiet.
3. **Inspect and compare at buff-bar scale:** run `inspect` (`CornersTransparent = true`) and `compare` at `64 × 64` (tooltip) and `32 × 32` (combat buff bar) beside `status-icons-in-combat-crop.png` and sibling powers in the character's set. **Completion criterion:** each power is immediately distinguishable by outer contour and dominant color at `32 × 32`, the rough/motif symbol is faithfully translated into StS2 rendering, and the lower-right stack-number zone is unobstructed.
4. **Export two files per power:** upon approval, export `256 × 256` (`Sts2WD/images/powers/big/<power>.png`) and `64 × 64` (`Sts2WD/images/powers/<power>.png`) via proportional `Fit`.

## Recipe 6: `base + refs → Power Emblem` (Branch C-2)

Follow [Branch C-2](powers.md#branch-c-2-base--refs--powers-focused-power-emblem-route). Open and pass the official StS2 status icon anchor ([status-icons-in-combat-crop.png](../../../../docs/research/sts2-style-refs/future-assets/status-icons-in-combat-crop.png)). When a user-supplied motif rough or symbol PNG exists in `docs/characters/<char>/refs/powers/`, pass it directly as `<ROUGH_OR_REF_INPUT>` and preserve its chosen symbol rather than reinterpreting from the power title alone.

```text
Use case: stylized-concept / reference-guided-generation.
Asset type: single symbolic status emblem for <OUTPUT_CONTRACT> (256x256 master exported to 64x64).
Engine mode: <ENGINE_MODE>.
Input owners:
- Motif / symbol rough (<ROUGH_OR_REF_INPUT in refs/powers/, when present>): owns the primary symbol choice, silhouette, and motif arrangement, incorporating any documented user overrides in <char>-art.md.
- Official StS2 status anchor (docs/research/sts2-style-refs/future-assets/status-icons-in-combat-crop.png): guides compact symbolic abstraction, bold interior/edge value separation, and 32-64px buff-bar readability; exclude composited stack digits, health bar, and unrelated symbols.
Subject: <ASSET_SUBJECT derived from the motif rough and character's visual theme>.
Composition: 2-3 bold interlocking shapes, a distinct outer silhouette, and <PALETTE>. Place the primary identifying feature in x/y:15-80% (central/upper-left) and keep the bottom-right x/y:60-100% stack-number zone quiet.
Style: <STYLE>. True transparent background. No miniature card scene, full figures, painted numbers, letters, box/frame or scene background.
```

Compare at `64 × 64` and `32 × 32` beside `status-icons-in-combat-crop.png` and sibling powers in the character's set.
