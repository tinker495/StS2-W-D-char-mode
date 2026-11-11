# Realta Card Illustration Art

Card portraits are automatically bound at runtime via convention: `res://Sts2WD/images/card_portraits/realta/<card_name>.png`. Delivered textures are the single source of truth; post-delivery visual narratives are pruned to prevent documentation bloat.

## Generative Art Direction & Native Reference Weighting

Card generation prioritizes official Slay the Spire 2 native card references ([density survey](../../../../research/sts2-card-illustration-density.md)) as primary anchors for rendering density, value planes, and thumbnail readability at 250×190. Pair the character visual anchors ([base.png](../../base.png): teal robe, ivory fur cuffs, bronze vambraces, broom-glaive) with one of the following visual archetypes:

1. **Direct Strike (단일 강타)**: Close weapon arc, asymmetric cutting impact, bright energy slash across quiet dark burgundy/navy backdrop.
   - *Official anchors:* `bludgeon`, `thrumming_hatchet`, `severance`.
2. **Kinetic Sweep (광역 기동)**: Sweeping diagonals, dynamic multi-target trails, clean negative space.
   - *Official anchors:* `whirlwind`, `echoing_slash`, `dagger_spray`.
3. **Craft & Tool (공방/도구 작업)**: Close-up on Realta's gloved hands, bronze vambraces, tools (hammer, tongs, whetstone), immersion in water/steam, anvil support. Exclude full face/body.
   - *Official anchors:* `glasswork`, `times_up`, `scrawl`.
4. **Tactical Guard (방어/결계)**: Solid defensive postures, barrier arcs, grounded mass.
   - *Official anchors:* `body_slam`, `dash`, `rolling_boulder`.
5. **Arcane Domain (각성/지속 효과)**: Persistent symbols, witch circles, radiant flame or forge light.
   - *Official anchors:* `automation`, `blight_strike`, `neurosurge`.
6. **Ancient Full-Art (고대 전면화, 606×852)**: Strict 3-zone vertical layout (`y:15–58%` focal window, `y:62–100%` quiet text background).
   - *Official anchors:* `corpse_explosion`, `hyperbeam`.

## Composition & Quality Invariants

- **Standard Cards:** `1000×760` opaque PNG (`25:19`), designed for `250×190` display.
- **Safe Zone:** Essential action, hands, and impact must reside inside `x:15–85%, y:12–82%`, clear of top-left energy orb and card borders.
- **Value Hierarchy:** 2–3 broad tonal planes. Foreground action/tool at 100% sharpness and contrast; midground target 60%; background 25–35% grouped values.
- **Ancient Full-Art:** Vertical `606×852` (`101:142`) for `realta_iordina_mac_bethlehem.png` and `realta_genius_inspiration.png`. Clear of top banner (`y:0–14%`); dark and quiet lower zone (`y:62–100%`).
