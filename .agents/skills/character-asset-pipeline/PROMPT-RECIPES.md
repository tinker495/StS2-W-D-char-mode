# Shared Prompt Slots and Recipe Index

Start with [SKILL.md](SKILL.md) and the selected route. Recipes live beside their execution procedure; this file is only the shared placeholder glossary and navigation index. Read the glossary only when a placeholder is unclear.

| Needed prompt | Authoritative recipe |
|---|---|
| New canonical base (1-A) | [Base route](routes/base.md#1-a-refs--base-new-standing-master) |
| Combat pose (1-C), chibi/UI/selection (3–4) | [UI route](routes/ui.md) |
| Relic (5) | [Relic route](routes/relics.md#recipe-5-base--refs--relic-icon-branch-c-1) |
| Power (6) | [Power route](routes/powers.md#recipe-6-base--refs--power-emblem-branch-c-2) |
| Standard/Ancient card (7–8) | [Card route](routes/cards.md) |
| Selected-image edit (1-B) or cleanup (2-A) | [Review route](routes/review.md) |
| Rough/reference translation (1-D), source labels | [References](references.md) |
## Shared Input Slots

| Slot | Source |
|---|---|
| `<CHARACTER_ANCHOR>` | Visible parts of the canonical appearance, face, apparent age, and equipment in the character art specification |
| `<IDENTITY_CUES>` | Face, hair, and costume identifiers retained in the portrait |
| `<CHIBI_DETAILS>` | Character-specific proportions, expression, and silhouette within the common chibi scope |
| `<ROUGH_OR_REF_INPUT>` | User-supplied rough sketch, symbol draft, object photo (`refs/`), or agent-staged layout/pose rough (`drafts/`), with adopted vs. excluded traits |
| `<OFFICIAL_ANCHOR>` | Same-use-case official StS2 reference image(s) from [sts2-style-refs](../../../docs/research/sts2-style-refs/README.md) (or temporary local PCK extraction), guiding display-size density and value grouping; framing only when no user rough or selected composition owns it |
| `<ENGINE_MODE>` | Engine-specific invocation (`Codex / GPT Image` via `image_gen` with `referenced_image_paths` and `transparent_background`, `Antigravity / Nano Banana` via `image-generator` subagent brief, or dual-engine comparison; see [engine instructions](engines.md#image-generation-engines-codex-gpt-image-vs-antigravity-nano-banana)) |
| `<PALETTE>`,`<MATERIALS>` | Color roles and materials in the character art specification |
| `<ASSET_SUBJECT>`,`<SCENE_SENTENCE>` | Character themes, recurring objects, and chosen action |
| `<FRAMING>`,`<OUTPUT_CONTRACT>` | Target composition, UI overlap, and asset-guide dimensions/paths |
| `<STYLE>` | Shared Block A in ARTS.md section 7.2, adapted to the use case |

Do not fill slots with another character's example values. Write reusable descriptions of appearance, composition, palette, and preserved constraints in the character document. Full generation calls may be kept in optional Git-ignored drafts; assembling a prompt must not require those records.
