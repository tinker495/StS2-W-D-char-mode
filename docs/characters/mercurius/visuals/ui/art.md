# Mercurius Branch B UI and Standing Art

## Scope and authority

On 2026-10-31 the user adopted selection tile B and the calmer-eye revisions for Branch B. The approved nine-image set is applied to runtime resources as described in the [delivery record](assets.md). Canonical identity remains owned by the unchanged [base specification](../base/art.md). The separate selection scene is specified [here](../selection/art.md). Tile A is an unselected comparison only.

## Composition and palette

The user's subsequent 2026-10-31 request asks for generally calmer eyes. Current portrait, marker and both tile candidates use slightly lowered upper lids and relaxed eyebrows, preserving green irises, gaze direction, head angle and the existing small smile. Keep them quietly attentive rather than sleepy, sad or sharply challenging. Standing continues to use the unchanged approved base.

| Asset | Direction and selection |
|---|---|
| Portrait | Head-dominant chibi, shallow three-quarter toward screen-right, rounded warm face, simple green eyes, broad ash-blond locks, navy/gold feather hat and a small mustard scarf. No hands or flute. |
| Portrait outline | White 2 px expansion of the delivered 128 px portrait alpha; identical pose and contour source. |
| Map marker | Separately painted chibi head and hat only, without scarf, neck, shoulders or pin border. Preserve the portrait's expression and palette. |
| Energy | One circular ochre-gold rim around a deep teal inset, with a small leaf-like outer notch. Quiet center for the numerical overlay; no painted number. Use one master for both sizes. |
| Selection tile A | Eye-level, right-facing three-quarter, lowered chin and gentle half-smile. Face-close non-chibi anatomy. |
| Selection tile B | Adopted: slightly low camera, left-facing three-quarter, raised chin, small smile and calmer eyes. The exposed head/scarf silhouette remains legible in the locked state. |
| Locked tiles | Black figure on gray, derived from the same resized alpha as the corresponding normal tile. No independent generation. |
| Standing/combat | Complete base painting resized proportionally, including feather, flute, fabric tails and soles. Applied as the shared combat, rest and merchant texture. |

Tiles intentionally let the hat and feather exit the generation frame. They are newly painted close views, not extracted regions of the base. Adopted normal background is `#254A53`; locked background is `#777777`. These delivery colors do not change model `NameColor`.

The base already shows separated, planted feet, rightward attention and both hands engaged with the flute. This active musical stance supplies the adopted static combat idle and ordinary standing export. Reuse preserves the approved equipment without inventing a crossbow stance. Runtime scene placement is documented separately; asset approval does not establish live-game validation.

## Reference roles and review

- The approved base owns face, apparent age, costume and palette. Portrait/marker deliberately use the shared chibi scope; tiles and standing retain canonical proportions.
- Official portrait/top-bar images guide small-scale recognition; their skull mask and UI frame are excluded. Official energy images guide rim/inset contrast; their digits are excluded. See [official reference index](../../../../research/sts2-style-refs/future-assets/README.md).
- All ten native normal/locked tiles were inspected through a whole-image board. Silent/Defect guide tile A; Necrobinder/Silent guide tile B. Native camera and information density are references, not character identity. See [tile study](../../../../research/sts2-character-select-tile-study.md).
- Fresh installed-game `idle_loop` time-zero renders of Silent and Necrobinder were compared with standing at 240 × 360 using SubjectFit. This comparison includes equipment in subject height and does not establish equal body scale in game.

The portrait received one corrective generation to enlarge/simplify the chibi face and reduce shoulder detail. The marker and energy each used one generation. Tiles used two contrasting cameras, with no corrective generation. Whole-image light/dark reviews at 48/85 px, 24/74 px and 132 × 195 confirmed recognizable hat/hair/eyes, a quiet energy center, and matching normal/locked contours. Hair and feather still contain more linework than the official skull portrait, but they remain readable at the reviewed sizes. No post-generation cropping, Cover fitting or alpha-threshold background removal was used.
