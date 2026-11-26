---
name: character-art-standing
description: Produce standing, combat and merchant character sprites from an approved character base.
---

Follow the [shared inputs, engine selection and delivery](/.agents/skills/character-asset-pipeline/SKILL.md#shared-production-contract) for the requested scope.

For ordinary standing art, first inspect the approved base for grounding and hand-to-equipment contact. Fit the entire base when it already serves the requested pose. Use Recipe 1-C for a distinct combat pose.

### 1-C. `base → Combat Pose` for Branch B

First apply the reuse decision in [ARTS.md 5.1](/ARTS.md#51-combat-rest-and-merchant-standing-art). A combat-ready base supplies both standing and combat through whole-image Fit, without generation. Use this recipe only for a noncombat/everyday base or a user-requested distinct combat pose. Then use the approved base for identity and original-game combat sprites for stance/readability. The new pose belongs to this production run; previous delivered combat sprites are not generation inputs.

```text
Use case: stylized-concept.
Asset type: isolated full-body combat-ready idle sprite for <CHARACTER>, distinct from ordinary standing art.
Input roles: approved base.png owns identity, apparent age, anatomy, equipment construction/proportions, palette and rendering; official combat references (installed-build Spine renders when available) guide camera, body/head orientation, gaze, grounded readiness and display-size clarity.
Subject: <CHARACTER_ANCHOR> with the approved base's canonical non-SD proportions.
Combat pose: <weapon-appropriate stance, planted feet, knee bend, weight distribution, torso engagement, exact hand contacts and grip spacing>. Keep the pose stable enough for a reusable combat idle PNG. Design it for combat use while preserving the base's identity; determine the base's existing pose from the actual image.
Body direction: <torso and pelvis orientation relative to the camera and combat target>.
Head angle: <visible facial planes and near/far eye visibility>. Preserve the approved facial identity.
Gaze: <pupil direction toward the combat target, independent of head rotation>.
Composition: complete figure AND complete equipment, separated from the face, with clear transparent margins. Generate directly at the intended aspect ratio; no foreshortening that disguises equipment length, clipping, or later cropping.
Style: <STYLE>. Materials and palette: <MATERIALS, PALETTE>. Match the approved rendering and actual display size.
Output: <OUTPUT_CONTRACT>, true transparent alpha. No environment, ground shadow, aura, motion trail, particles, UI, words, frame or watermark.
```

Compare the new combat pose against the ordinary standing export at the same display size. Inspect balance, both hands, equipment continuity, silhouette, margins, face identity, and gaze on light/dark backgrounds. Save candidates separately; runtime promotion follows the user's approval scope.
