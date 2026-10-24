# Althea Art Decision History

The [art direction](althea-art.md) owns the current appearance; the [asset status](althea-assets.md) owns the latest candidate and its scope. This file records judgments not to repeat and their reasons. The user adopted a draft base on 2026-10-24; see the selection record below.

## 2026-10-23 — Combat Camera and Equipment Connections

The user-supplied `refs/ref1.jpg` is a bust reference showing silver-white hair, blue eyes and silver armor. Lower body and weapon are not visible, so their design stays provisional. The user said the first candidates' angle did not match the original combat standing sprites. The official Defect and Ironclad combat images and Silent animation frame 40 then became the camera and pose references. Roles were separated so the bust's camera would not be extended into the full-body composition.

The user asked for a fully drawn sword and a head angle between frontal and profile. Edits aligned the sword axis and both hands' contacts, with the other hand supporting the empty scabbard. The head became a right-facing three-quarter view with the far eye and part of the far cheek visible, the gaze still on the enemy at right. Drawn-sword attempts that hid the other hand, and edits that introduced background smudges, were rejected. The head-angle revision reviewed then was not approved as the final base either.

## 2026-10-24 — Strengthening the Original Rendering

The first two candidates of the new Branch A run were regenerated from the reference and combat images. The first read more clearly right-facing but pointed its scabbard down-right; the second showed the empty scabbard and both hands' roles but turned its torso closer to frontal. Both carried many fine hair strands and armor reflections and left a narrow margin at the sword tip. The user said both were far from the original style.

The next generation rewrote the prompt to make the official images the main rendering inputs and take only appearance and costume cues from the bust. It stressed large hair masses, broad armor values with few reflections, and simple eyes and skin shading. Input order and prompt changed together, so the improvement cannot be attributed to input order alone.

The first simplified result turned the face to full profile and was rejected. The next restored a partly visible far eye and the margins around the equipment. Information density moved closer to the originals, but value edges became harder and brushwork sparser. Regeneration also changed the hair tie and the equipment's construction and proportions, so those differences were not treated as approved design.

Original comparisons at the time Fit whole research images into an intermediate canvas, without cropping or background removal. Because that canvas size could be mistaken for source resolution, the tool gained a mode that reads originals directly, fits each individually, and labels source size and scale.

## 2026-10-24 — Draft Base Selection and Rejection of Over-Simplified "Style-Pure" Rules

After the clean 2D simplification run produced `base.png` (passing `refs/ref1.jpg` directly as an image input alongside official combat references), a retrospective experiment tried an extreme "style-pure" rule: withholding `ref1.jpg` from image inputs (transcribing it to text only), forcing compact `6–6.5`-head proportions with chunky hands/boots, and compressing the bright silver-white/white palette into rough pewter/bone gouache.

The user explicitly rejected that `style-pure` candidate and the guidelines that produced it as too crude and clunky (`투박함`), and confirmed preference for the clean, graceful 2D style of `base.png`:

- **Keep character references as direct image inputs.** Withholding `ref1.jpg` degraded facial appeal, hair flow, and knightly silhouette into blunt cartoon shapes. Passing `ref1.jpg` (and now `base.png`) directly as an image input while instructing the generator to group photorealistic strands and noisy highlights into clean 2D planes preserves both appeal and readability.
- **Preserve natural, graceful proportions.** Ironclad and Defect's bulky or compact builds are character-specific and must not be forced onto slender or young-adult characters.
- **Preserve clean palette brightness.** Althea's bright silver-white hair, clean silver steel plates, and warm-ivory/white tabard are core to her appeal; compressing them into dark ash/pewter gouache makes the sprite look muddy and rough.
- **Retained observation tooling.** The non-destructive observation improvements from the retrospective remain active: `inspect` reports `MarginsPx` and `MinMarginRatio` immediately after generation, `compare` supports `SharedScale`, `IndividualFit`, and `SubjectFit`, and `official-poses` renders unoccluded Spine poses from the installed game for camera and stance reference.

The user-preferred painting was promoted unchanged to `base.png` as the canonical draft base. Runtime assets were not changed.