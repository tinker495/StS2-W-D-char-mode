# Reference Roles and Landmark Briefs

Read the source-label section when adopting or changing character features. Read the rough section whenever a supplied sketch, layout guide, object photo or motif determines the result. Open every actual input; never replace an available visual reference with text alone.

Store supplied roughs, motifs and object photos under `docs/characters/<char>/refs/` (optionally `refs/cards/`, `refs/relics/`, `refs/powers/`) and record adopted/excluded traits in `<char>-art.md`. Optional agent-staged layouts belong in ignored `drafts/`. Shared documents must explain current constraints without those local drafts.

## Source Labels and Character Orientation
For character orientation, write body direction, head angle, and gaze as separate prompt fields. Describe head angle through the visible facial planes and the extent of the near/far eyes, using the target specification and references. A screen direction alone does not determine head rotation.

**Source labels:** record each adopted feature as *user-confirmed* (a user decision), *reference-observed* (seen in a raw reference; evidence, not approval) or *provisional* (an agent choice absent from confirmed references). Preserve by these labels. A provisional choice stays provisional even when the output looks successful, and is reassessed when the use case changes.

## 1-D. Rough Sketch or Reference Image → Finished Asset (`refs/` or `drafts/<guide>.png`)

Use whenever the user supplies a rough sketch, symbol draft, silhouette, or object/photo reference (`refs/<category>/` or chat attachment), or when you construct a layout/pose rough in `drafts/<asset>-rough.png` to lock complex staging before final rendering. Combine this block with the selected route's asset recipe.

- **Always pass the rough or reference file directly as an image input.** Never discard it in favor of text-only description.
- **Write a landmark brief before generation:** record camera/body direction, visible body extent, head/gaze, each hand's position and role, and the object's grip-to-tip direction and contact point. Use rough-relative locations and visible relationships; label any necessary interpretation as an assumption. Preserve these relationships when adapting the canvas ratio by extending the setting around them. Keep the rough first among composition inputs and label each reference's role in the prompt.
- **Assign explicit, non-overlapping owners:**
  - **Rough sketch / layout guide:** owns primary camera angle, silhouette, pose/gesture, focal placement, and core symbol arrangement. Translate rough lines and flats into finished StS2 2D game art without reinterpreting a user-chosen motif from the asset title alone.
  - **Object / photo / motif reference:** owns structural construction, proportions, and functional identifiers. Simplify photorealistic grain, complex engraving, background clutter, lighting mismatch, and watermarks into 2–3 broad value families and crisp contours.
  - **Approved `base.png`:** owns character face, apparent age, proportions, costume, and palette. It supplies weapon construction only when that weapon belongs in the requested scene. The user's card-specific prop description and its rough/object reference own a different weapon's identity and handling; carry the latest correction into subsequent edits.
  - **Current-run object-family masters:** own the recurring prop's construction within the requested object family.
  - **`<OFFICIAL_ANCHOR>`:** owns target display-size information density, contour hierarchy, and safe-zone readability.

After generation, compare the rough and candidate directly: mark each landmark preserved, changed, or uncertain with visible evidence. A matching theme or plausible pose does not resolve a reversed object direction, substituted weapon, changed hand action, or lost body extent. Correct those findings before surface polish; return to the rough when the candidate has drifted. Record remaining differences rather than claiming exact rough fidelity.

```text
Use case: reference-guided-generation / identity-preserve.
Asset type: <target asset family and OUTPUT_CONTRACT>.
Engine mode: <ENGINE_MODE>.
Input owners:
- Rough / motif / object reference (<ROUGH_OR_REF_INPUT>): owns <composition / silhouette / pose / object construction / primary symbol arrangement>. Preserve <adopted rough/reference traits>; exclude <sketch construction lines, photorealistic noise, source background, border, or superseded details>.
- Character & prop anchors (<approved base.png and/or current-run object-family master, when applicable>): own <visible character identity and requested prop construction, separately assigned; omit unrelated base equipment>.
- Official StS2 anchor (<OFFICIAL_ANCHOR>): guides <display-size readability, 2-3 value plane grouping, contour weight, and UI safe-zone framing>; exclude <official character/item identity, UI frame, numbers, and dithering>.
Subject & staging: <translate the rough/reference directly into the finished asset subject and action without reinterpreting the chosen motif>.
Landmarks: <camera/body direction and visible extent; head/gaze; hand positions and roles; requested object type, grip-to-tip direction, and contact point>.
Style: <STYLE>.
Output: <OUTPUT_CONTRACT>.
```
