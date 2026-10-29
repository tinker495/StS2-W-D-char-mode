# Branch D: Review, Cleanup, and Conversion

Read the selected source and requested change, ARTS.md 1.3, 2.2 and 7.1, then [the shared review loop](../workflow.md#default-generatereviewcorrect-loop). Read the active [engine section](../engines.md) only when regenerating. Read character/asset sections only for visible features or the affected output contract; a local edit does not require a new production inventory. Comparison-only or conversion-only requests use [observation tools](../observation-tools.md) and [conversion commands](../scripts/conversion-tools.md) without image generation. Cards retain ARTS.md 5.5's exact geometry and full painted coverage.

For style/density review, open a same-use-case official image and compare equivalent displayed areas. For a local edit, preserve the selected source and add only references needed for its changed part.

For a packaged texture, [`observe-texture`](../scripts/card-observations.md) exports the complete image and reports dimensions, opacity and optional source-pixel equality. For fresh native card references, use `official-cards` in that guide. The reports are observations; review the pixels and the requested scope before delivery.

Use `inspect` and `preview` to examine the master and actual display size first. Use Recipe 1-B for selected-image edits, Recipe 1-D when applying a user-supplied rough/reference correction, Recipe 2-A for surface cleanup, and export/outline for delivery. For comparison-only requests, choose the `compare` mode in [observation tools](../observation-tools.md) for the question being investigated.

### 1-B. Local Edits to a Selected Image

Specify inputs in this order: **exact user-selected source → changed axis → preserved features**. Open the actual filename, attachment, or version to confirm it, and use it as the sole edit target. Limit any additional reference to the relevant part.

- Separate axes such as eye direction/head rotation and length/width/thickness. Name the requested change and the minimum connected parts needed to make it coherent; preserve unrelated axes.
- Describe change relative to the selected source. If a numeric amount was not supplied by the user, state its observational basis and provisional status. Do not add unrequested absolute ratios or body-size targets.
- List neighboring axes that could be confused with the edit, plus preserved face, pose, character scale, framing, and style. Do not paste the full equipment specification or new-generation recipe again.
- For hand/equipment edits, identify each affected arm by its shoulder and near/far position, then trace shoulder → elbow → wrist → hand → contact target. Specify which joints must move and which remain fixed. Check that each required hand and its contact remain readable after the edit; screen-left/right hand labels alone do not establish the arm connection.
- For overlap edits, distinguish anatomical front/back from camera depth. In a rear view, equipment behind the character's back may be nearer the camera and visible over the coat. State which surface occludes which, where the object enters/exits the silhouette, and how its centerline continues through the grip. Preserve unrelated pose and equipment design.
- For requests to reduce detail, keep the selected image's staging and use Recipe 2-A. Identify the surfaces and background marks to merge; simplifying the prompt alone does not establish that the painting became simpler.

```text
Use case: precise-object-edit / identity-preserve.
Edit target: <exact user-selected image and version>. This image owns the current design and rendering.
Change only: <requested part and dimension, with a restrained change relative to this input; necessary connected joints/contacts when applicable>.
Depth and continuity: <camera-near to camera-far order at the affected crossing, visible/hidden segments, and hand/tool connection; for overlap edits>.
Body direction: <preserved or changed torso/pelvis orientation; for character orientation edits>.
Head angle: <preserved or changed facial planes and near/far eye visibility; for character orientation edits>.
Gaze: <preserved or changed pupil direction and target; for character orientation edits>.
Preserve: <neighboring dimensions and identity/pose/scale/framing/materials that must stay fixed, with their source labels>.
Output: the complete edited image in the same canvas and framing, preserving existing transparency.
```

After generation, use the [default review loop](../workflow.md#default-generatereviewcorrect-loop). Compare the selected source and result, checking the requested difference, depth/continuity, and preserved features. Correct clear defects within the bounded loop before the final recommendation; after unrelated drift, branch again from the selected source. Promotion follows the skill's existing approval scope.

## Recipe 2: Surface Passes

Do not generate again when the user requests applying the selected painting unchanged.

### 2-A. Surface Cleanup

Use when surface noise is visible at actual size or the user asks for less detail. Name the noisy regions, then specify the larger connected shapes that replace them: hair and fur clumps, broad cloth/metal planes, a few continuous liquid/effect shapes, and quiet background masses. Preserve the focal contact and silhouette. Compare source/result beside the official anchor at the same display size; record which competing marks disappeared and whether the action and pose stayed intact.

```text
Use case: identity-preserve / rendering-cleanup.
Edit target: <ASSET>. Preserve identity, equipment construction, pose, proportions, crisp facial features, framing and transparency.
Rendering cleanup only: <ARTS.md 7.2 Block D>. Merge <observed noisy regions> into <larger connected shapes>, keeping the strongest contrast at <focal action/contact>. Preserve clean cel-plus-soft-form transitions, crisp contours, and palette clarity. Do not redesign equipment, coarsen facial anatomy, dull colors, blur, or over-sharpen the image.
```
