# Realta Standing and Animation Art

- **Combat/merchant weapon depth:** draw the complete broom-glaive and ribbon in front of every body part, including head, hat and face. Only the hand enclosing the shaft and VFX sit above them. The released weapon also falls in front of the body during death and rests on the floor; its ribbon first streams toward the hat and then down so it avoids the eyes. The final blade lies below the face. [Animation ownership](../../animation/README.md) defines tree order, release and revival transitions.
- **Merchant free arm:** place the viewer-right free sleeve, cuff, forearm and glove behind the torso, front/back coat and legs. Show only a little forearm and glove edge beside the coat so the hand reads as resting behind the hip. Combat restores the relevant pose's front ordering, changing order when the arm has moved clear of the coat.

- **Combat rig contacts and motion:** preserve the visible face, clothing and equipment while completing the hidden sleeve, thigh, knee, ankle, neck and wrist surfaces. The attack encloses the shaft with the back of the gripping hand visible; hurt adds a small pelvis recoil; early death brings both knees forward into a folded pose. Front coat and ivory fur sit over the thighs; shoulder plates remain attached to the torso. [Animation ownership](../../animation/README.md) defines authoring inputs, coordinates, weights and clip contracts.

- **Merchant cutout animation:** the native `relaxed_loop` now uses a narrow planted stance with weight toward the viewer-left leg, the free glove resting by the belt/hip, and a low one-handed shaft grip. Keep the upper arm/elbow visible at full source length. The broom-glaive rests beside her with bristles just above floor support, the blade high, and its ribbon knot clearly outside the hat brim. Preserve the complete prop and fixed grip attachment. Use two restrained breaths over eight seconds with slow weight sway and small warmed spring motion; retain the youthful confident face. This animation-specific staging supersedes the old relaxed source keys, while the canonical base and standing still retain their approved composition. Technical coordinates, aliases and transition behavior are in [animation/README.md](../../animation/README.md).

- **Shared standing image:** use the complete [base.png](../../base.png), including transparent margins, without regeneration. `combat.png`, 384×576 RGBA, is shared by combat, rest, and merchant scenes. There is no separate runtime `standing.png`.
- **Current pose:** retain the base's upright, one-handed broom-glaive stance, youthful face, open forehead, paired pauldrons, and continuous collar. The separately generated two-handed combat pose was removed at the user's request. This character-specific delivery overrides the generic Branch B split until the user requests otherwise.
- **Application status:** combat/merchant display at 240×360 with scale 0.625; rest displays at 326.4×489.6 with scale 0.85. See the [asset guide](assets.md) for the rollback and verification limits.
- **Composition/margins:**
  - Three-quarter standing idle toward screen-right. **Forward means the body's battle direction, not facing the camera.**
  - Specify head and pupils separately. Look toward the opponent, not the viewer. Preserve both eyes, face width, forehead, and short jaw; do not rotate into profile or erase the far eye. Limit necessary head adjustment and do not force body/nose/pupils to identical angles.
  - Keep roughly 3–5% fully transparent margins beyond blade tip, soles, broom, and brim; prevent boundary clipping.
  - Retain the base's one-handed grip and keep the shaft and socket away from the face.
- **Scene wiring/placement:** the [asset guide](assets.md) owns current runtime anchors. This section owns face, pose, and margins.

## Attack Trail

Extend the existing teal/ivory trail along the actual blade path. Reveal it behind the blade tip in sequence, with a thick leading section and a tapered tail. Retain the path through the 0.15-second hit until 0.25 seconds, and remove it at 0.34 seconds. Keep the face visible and draw the trail above the weapon.
