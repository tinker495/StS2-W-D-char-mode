# Realta Appearance and Identity

## Roles and Conflicts of Four Raw References

The four originals in [refs/](../../refs/) have distinct adopted/excluded roles.

| File | Adopted role/features | Excluded features |
|---|---|---|
| [ref1.png](../../refs/ref1.png) | Primary identity/color/weapon: large bent petrol-blue hat, copper-red bob, violet eyes, confident smile, blue coat/ivory fur, broom-glaive with blade + metal socket + red ribbon + wood shaft + broom | Floating crossed-leg pose, one-sided shoulder armor, thin anime outlines and skin highlights |
| [ref2.png](../../refs/ref2.png) | Both bronze pauldrons, continuous ivory collar around both sides of neck, breastplate/belt/pouch construction, grounded two-foot stance | Desaturated gray-violet costume instead of petrol blue, long subdued face, fine metal/fur texture |
| [ref3.png](../../refs/ref3.png) | Visible pointed elf ear, playful brows/mouth, bronze shin/foot plate construction | Extreme leaning perspective/body exaggeration; straw resembling long hair spread across the floor |
| [ref4.png](../../refs/ref4.png) | Petite youthful build, small lively face, playful confidence; forehead guided by the user's request and ref2's part | Crouched/extreme perspective, low brim hiding face, teal palette replacement, intricate engraved armor; do not replace the approved rendering style |

## Canonical Base Specification

[base.png](../../base.png) is the user-approved 1152×1728 RGBA master and **single visual identity anchor**. Preserve the complete character, weapon, canvas, and transparent margins. The current combat/rest/merchant image is a proportional reduction of that whole file. The user reverted the separately generated combat pose; generating another pose is not part of the current delivery. Generate UI/selection scenes separately with purpose-specific cameras while preserving the following identity. Dimensions/conversions/status belong in the [asset guide](../ui/assets.md).

1. **Head, face, ears:** apparent age around 13–14, short soft jaw/cheeks, short copper-red bob parted toward both temples to expose a broad forehead and eyebrows. Violet eyes, a clearly pointed ear visible through the hair on the left side of the image, confident half-smile. Keep a two-eye three-quarter face and forward gaze without an extreme profile.
2. **Witch hat:** broad brim, crown bent/drooping left-back, deep petrol blue. Brown leather band and near-round bronze/brass front buckle. Dark navy-violet underside (`#253748`) supports the face.
3. **Shoulders/collar:** rounded bronze pauldrons on **both shoulders**, plus an **unbroken ivory fur collar** around both sides of the neck.
4. **Chest/arms:** matte curved bronze breastplate, small round/square upper connections, gently V-shaped lower center. Dark gloves and bronze vambraces on both arms; chunky ivory fur at cuffs/cape edges.
5. **Coat/waist:** petrol-blue A-line coat with ivory fur along both front openings and hem. Brown belt with bronze buckle; a rectangular flapped pouch with metal fastener at the character's front-right (viewer center-left).
6. **Legs/boots:** dark brown/charcoal leggings, diamond-shaped bronze knee guards, sturdy boots with folded leather cuffs and bronze shin/instep plates.
7. **Hybrid broom-glaive:**
   - A **single long polearm**, never a scythe or separate sword and broom.
   - Gently curved single-edged dark steel blade with a distinct pale bevel, following the approved base.
   - Wedge-shaped **bronze metal socket and two rivets**, not cloth/leather wrapping.
   - Bright red ribbon (`#B54535`) tied immediately below the socket, two flowing tails.
   - Aged wood shaft with an extremely restrained natural bow and only two or three long grain lines. Avoid smooth plastic straightness and severe branch-like zigzags.
   - Fan-shaped coarse dry straw, tied with red cord and **two dark bands**; a few broad bundles.
8. **Surface density:** broad connected color planes with selected angular values. Avoid accumulated tiny patches/lines in armor, fur, or wood. Greater official-style similarity does not authorize changing the approved face/silhouette.

## Expressions to Preserve

**A youthful 13–14 appearance and exposed forehead in natural full-body proportions.** This is apparent age, not a new canonical age declaration. Non-SD does not mean adult; do not default prompts to `adult`.

- **Face/build:** short soft lower face, mildly round cheeks, small simple nose, slightly larger head/body relationship and petite build, without the icon's2–2.5-head chibi anatomy. Preserve playfulness, confidence, violet eyes, and pointed ears. Judge age visually rather than by head count alone.
- **Hair/hat:** open part, hair swept toward temples, forehead visible from central hairline to brows. Long diagonal bangs or a low brim must not cover it again.
- **Style/equipment:** `base.png` owns broad connected planes, selected angular values, low surface noise, blue/bronze palette, paired shoulders, continuous collar, and single broom polearm.
- **Forward/head direction:** battle-forward is screen-right. Keep both eyes, short jaw, and open forehead while directing pupils forward. Large head rotation into profile changes identity. Follow [ARTS.md section 5.1](../../../../../ARTS.md#51-combat-rest-and-merchant-standing-art) and the [standing-art constraints](../animation/art.md).
- **Weapon ratio:** broom tip reaches the feet and blade tip rises above the hat, contrasting a small body with a long polearm. Preserve a shaft narrow enough for one-hand grip and a long narrow blade. Use `base.png` for exact length/width/thickness relationships. Changing one axis does not imply enlarging the whole weapon or shrinking the character.

See [history](../../realta-history.md) for reasons and the [asset guide](../ui/assets.md) for delivery status.

## Canonical Identity Anchor

```text
Character anchor (match supplied approved base.png): Realta, youthful female elf witch-artisan warrior with an apparent age around 13-14 and natural non-SD proportions. Short copper-red bob (#C8432B) parted back toward both temples to reveal a broad open forehead, violet eyes (#9273B4), clearly pointed ears, soft short jaw and confident lively expression. Wide-brimmed petrol-blue witch hat (#245F80) with a left-bent crown, brown leather band and round bronze buckle. Continuous ivory fur neck collar (#EDE6D6), BOTH shoulders protected by rounded weathered bronze pauldrons (#9D754E), clean bronze breastplate and forearm vambraces, petrol-blue coat and cape with chunky ivory fur trim, brown leather belt with rectangular front pouch, dark gloves, dark leggings, diamond bronze knee guards and sturdy bronze-plated boots.
Weapon anchor (when visible): a SINGLE hybrid broom-glaive polearm — upper curved dark steel glaive blade with pale edge bevel, connected via a wedge-shaped bronze metal socket with two rivets, a red ribbon (#C43428) tied just below the socket, a long aged wooden shaft with a very subtle natural bow, and a lower broom bundle of coarse golden-brown straw bound with red cord and two dark bands.
```
