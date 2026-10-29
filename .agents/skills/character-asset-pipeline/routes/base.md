# Branch A: `refs → base`

This route establishes a standing master. For a canonical Ancient/event painting, use [event art](event.md#event-painting-and-canonical-base); the filename `base.png` does not prescribe standing geometry.

Read the target identity/reference roles and current base status, then ARTS.md sections 1–4 and 5.1. Use [the shared workflow](../workflow.md), the active [engine section](../engines.md), [source labels](../references.md#source-labels-and-character-orientation), and the recipe below. A rough also requires the landmark brief in references.md. Official standing extraction and comparisons are in [observation tools](../observation-tools.md#official-standing-poses).

1. Open all raw references in `refs/` and the official combat/merchant references (`official-poses` or [sts2-style-refs](../../../../docs/research/sts2-style-refs/README.md)). Record adopted and excluded features in the target art specification with [Recipe 1's source labels](../references.md#source-labels-and-character-orientation). For revisions, separate preserved features from the requested change axis.
2. Generate with Recipe 1-A (and 1-D when combining a separate pose/layout rough with an identity reference), passing the raw character reference directly as an image input so facial appeal, sleek proportions, and costume construction stay faithful while translating surface micro-noise into clean 2D game art. Use [Recipe 1-B](review.md#1-b-local-edits-to-a-selected-image) for a local edit to a selected image.
3. Starting with the first candidate, run `inspect` and open a `compare` sheet of the complete candidate beside same-use-case official references at the intended display size ([comparison modes](../observation-tools.md); `SubjectFit` for style/silhouette, `SharedScale` for preservation). Review in this order: (a) camera, body direction, head angle and gaze; (b) action and weapon/equipment state; (c) silhouette, grounding, margins and hand/equipment connections; (d) clean 2D rendering, facial appeal, sleek proportions, and detail density. **Completion criterion:** the sheet is opened, the principal differences are recorded, every Recipe 1-A after-generation observation is filled, drift is recorded as unresolved, and `MinMarginRatio` is checked against ARTS.md 5.1. Resolve composition or use-case mismatches before surface cleanup or recommending a candidate. File measurements do not establish visual agreement. Branch from clean references or a selected source rather than repeatedly editing a rejected result.
4. If accumulated edits or surface noise are visible, consider [Recipe 2-A's cleanup pass](review.md#2-a-surface-cleanup) before promotion. If the user requests applying the final painting unchanged, preserve it without additional generation.
5. Promote the approved painting to the requested `base.png` and update the current-status table. Describe appearance changes, runtime coverage, and remaining differences. Record dimensions, procedure, and observed pixel changes for delivery conversions such as transparent padding. Export runtime standing art only if requested.

### 1-A. `refs → base` New Standing Master

Read each reference's adopted/excluded roles in the art specification. If a base exists, distinguish preserved identity from the newly designed composition.

Before drafting the prompt, assign an input owner for each axis: identity/equipment, camera, pose/direction, and rendering. Pass the raw character reference directly as an image input for identity, facial appeal, proportions, and costume construction; use approved character bases (such as `docs/characters/realta/base.png` or `docs/characters/althea/base.png`) or official sprites as needed for clean 2D game-sprite readability and grounded combat framing. Do not withhold the character reference (`style-pure` text-only generation) or force a rough gouache/chunky-proportion conversion that degrades the character's facial appeal, sleek build, or clean palette. Resolve conflicts between inputs in this assignment (for example, expanding a bust reference into a full-body combat stance), then make the prompt follow it. A bust reference supplies appearance, not a default full-body camera. Branch A establishes a canonical base; its name does not prescribe a frontal or relaxed pose.

Record the actual image-input list in the before/after notes.

Keep a short before/after record in optional draft notes. Define concrete items before generation; after generation inspect those same items in the comparison sheet and fill every observation. List equipment dimensions and hand contacts as separate items.

| Treatment | Before generation | After generation |
|---|---|---|
| Change | Requested axis, intended difference, and source of the request | Observed change and remaining mismatch |
| Preserve | Visible features and their user-confirmed or reference-observed basis | Retained features and any unintended drift |
| Provisional | Agent-selected details absent from confirmed references | Resulting interpretation, still unapproved |

Rewrite the prompt around the current objective instead of appending another priority block to an old prompt. Lead with concrete rendering observations from the relevant originals, then preserved identity, composition/contacts, and output. Prefer positive descriptions of the intended shapes and materials; retain only exclusions needed to prevent specific confusion or scope violations. Input order can express intent but is not proof of causal influence on the result. Recipe 1-B retains its selected-source-first order for local edits.

```text
Use case: stylized-concept / identity-preserve.
Asset type: full-body standing master for <CHARACTER>.
Input owners: identity/equipment/proportions = <raw character reference and CHARACTER_ANCHOR>; camera/pose/direction = <requested stance and same-use-case reference>; rendering = <clean 2D cel-plus-soft-form sprite style from ARTS.md Block A>. Image inputs: <actual list>. <Adopted/excluded features; preserve approved identity/rendering when present>.
Rendering target: <clean 2D cel-plus-soft-form shading, crisp dark material-colored contours, smooth faceted metal, and clean fabric/hair values from ARTS.md Block A for the target display size>.
Subject: <CHARACTER_ANCHOR>. Preserve the character-specific apparent age, sharp facial appeal, and sleek canonical proportions.
Materials and palette: <MATERIALS, PALETTE>. Preserve: <confirmed visible features>. Provisional: <unconfirmed design choices>.
Composition: <FRAMING, battle direction, grounded stance, tool placement and clear margins>. Design the entire final canvas directly, with all required extremities inside it.
Body direction: <torso and pelvis orientation relative to the camera and target>.
Head angle: <visible facial planes and near/far eye visibility>. Preserve the approved facial anatomy and apparent age.
Gaze: <pupil direction and attention target, independent of head rotation>.
Proportions: <sleek canonical body proportions from the character specification and ARTS.md 2.4; equipment proportions supported by the references and user choices>.
Output: <OUTPUT_CONTRACT>; transparent background where required. No floor shadow, scenery, glow, UI, text, frame or watermark.
```
