# Character Two Asset Status

## Relic Candidates (2026-10-24)

Five new Althea relic candidates are staged in `relic-candidates/2026-10-24/` beside this guide. Each slug below has a 94 x 94 `<slug>.png`, a white 2 px alpha-derived `<slug>_outline.png`, and a 256 x 256 `big/<slug>.png`: 15 delivery PNGs total. `overview.png` shows all five whole compositions with 94/48 px light/dark previews. All were generated separately with the built-in image tool. Large exports proportionally fit the whole original canvas; small exports fit it to 90 x 90, centered with 2 px transparent padding in the 94 px canvas before outline generation. No crop or region extraction was used.

| Model | Korean title | Candidate slug |
|---|---|---|
| AltheaWyrd | 윌드 | `althea_wyrd` |
| AltheaBloodFrenzy | 피의 광란 | `althea_blood_frenzy` |
| AltheaFightersBracer | 투사의 완갑 | `althea_fighters_bracer` |
| AltheaCrekinMacDubh | 크레킨 막 두브 | `althea_crekin_mac_dubh` |
| AltheaAncestorsRecognition | 선조의 인정 | `althea_ancestors_recognition` |

The user photos establish the sword, bracer and black-cuirass subjects. The accepted crimson energy master supplies the Rare relic's hexagonal rim/circular-center shape with a new blue treatment. The original-game relic/potion reference supplies broad values and contour density only. Physical descriptions and exclusions are recorded in the art guide; optional generation masters/prompts are under `drafts/relic-refresh/`.

This is a candidate-only delivery: runtime PNGs, code, installed game and PCK remain unchanged. `AltheaRelic` currently overrides all three icon paths with native Burning Blood art. After the user selects candidates for application, remove those presentation overrides so the inherited `Sts2WDRelic` model-filename paths resolve the 15 delivered exports; retain all gameplay hooks and saved IDs. Build and resource-load verification belong to that application step. No build or gameplay test was run for candidate artwork.

## Energy Icon Draft

The runtime uses the accepted crimson variant at `Sts2WD/images/charui/character_two/big_energy.png` and `text_energy.png`. `CharacterTwoCardPool`, `CharacterTwoRelicPool`, and `CharacterTwoPotionPool` point to these shared files. These cover card-cost and inline energy textures. The combat energy counter still uses the placeholder character's native presentation.

The user requested a dark-crimson recolor of the Necrobinder shape and accepted the resulting direction. The generated review master is 1254x1254 RGBA with transparent corners; complete-image Fit conversions are 74x74 RGBA for card costs and 24x24 RGBA for inline energy. The accepted variant is not a pixel-exact hue transform. Runtime replacement and game-folder installation were performed on 2026-10-23. Preserve the complete symbol and alpha; do not crop the painting. Optional local master, conversions, and comparisons are under `drafts/energy-crimson-20261022/`.

To recover the original reference from the installed game, mount `SlayTheSpire2.pck` in Godot, load `res://images/atlases/ui_atlas.sprites/card/energy_necrobinder.tres` as `AtlasTexture`, decompress its atlas image and read its declared region. This recovers the asset's native region, rather than deriving a new composition from a larger painting. The inline source is `res://images/packed/sprite_fonts/necrobinder_energy_icon.png`.

## Red-to-Navy Card Frame Preview

The user requested a red upper frame, subsequently revised to clearer crimson transitioning toward black-blue below. A 598x844 RGBA body and a 1120x620 three-rarity comparison were rendered on 2026-10-22. Optional outputs are `drafts/card-frame-red-navy-v2-20261022/frame.png`, `red_navy.gdshader`, and `preview.png`. The accepted body was copied to `Sts2WD/images/cards/character_two/frame.png` on 2026-10-23 and connected through `CharacterTwoCardPool.CustomFrame`. The pool accent color is `#a91f30`. This frame was installed locally on 2026-10-23.

The source body is the native `res://images/atlases/ui_atlas.sprites/card/card_frame_skill_s.tres`, recovered from its decompressed atlas with the declared region and margin. Retain its full canvas and alpha. The draft uses a canvas-item shader with upper RGB `(0.67, 0.12, 0.16)` and lower RGB `(0.065, 0.13, 0.22)`, interpolated by `smoothstep(0.05, 0.9, UV.y)`. Multiply by `clamp(dot(source.rgb, vec3(0.2126, 0.7152, 0.0722)) / 0.29, 0.52, 1.6)` to preserve the native bevels and quiet surface texture. Apply the shader to a standalone texture so UV coordinates cover the whole frame rather than its atlas.

The comparison uses the actual game's `card_banner` and `card_portrait_border_skill_s` textures with the original Common/Uncommon/Rare banner materials and Korean font, plus the crimson energy variant displayed at 64px. The illustration area and effect text are placeholders. This comparison is a resource-based SubViewport preview, not a live `NCard` check. Runtime `RealtaCardAppearance` now also handles Althea: native rarity layers remain visible, body tinting is disabled for the precolored texture, and the type plaque uses `Sts2WD/materials/character_two/type_plaque.tres` with `dark_metal.gdshader`. Cost autosizing uses 18–26px with a 3px outline and native positioning. Ancient cards retain their native special layers. Final in-game UI review remains necessary.

## Temporary Portrait Palettes

`Sts2WDCode/Characters/CharacterTwo/AltheaPortraitAppearance.cs` applies rarity palettes to the `%Portrait` texture of visible Althea cards after native `NCard.Reload`. It uses `Sts2WD/materials/character_two/portrait_common.tres`, `portrait_uncommon.tres`, and `portrait_rare.tres`, sharing `portrait_palette.gdshader`. No native raster or atlas is copied or edited. Existing Attack, Skill, Power, and Curse placeholder sources remain selected by `AltheaCard.Art`.

The shader preserves source alpha and vertex/canvas modulation. It computes luminance with weights `(0.2126, 0.7152, 0.0722)`, raises it to `0.85`, and uses two smooth transitions between shadow, middle, and light colors. The standard transition reaches its middle at `0.55`; Rare holds shadows through `0.22` and reaches gold at `0.65` to retain a black background. Palette RGB values are explicit in the three material files. No randomness, crop, scaling, or composition change is applied at runtime.

Native `NCard.Reload` assigns blur for undiscovered cards; the palette patch leaves non-null materials untouched. Native reload/pool reset clears the palette when a node displays another card. Ancient full-art cards are excluded. Native rarity banners, portrait borders, and frame layers remain independent.

A 840×520 comparison was rendered using complete native Ironclad Strike and Defend portraits at their actual 250×190 size, arranged as two rows and three rarity columns. The comparison checks the same shader/materials used at runtime; it is not a live card-node or gameplay check. Runtime source wiring and packaging are updated, but this portrait palette change has not been installed into the game folder. Earlier card/energy assets were installed on 2026-10-23.

## Wraith Token Portrait

`AltheaCard.Art` selects the native `Soul` model for `AltheaWraith`; all other placeholder source selections remain as above. `AltheaPortraitAppearance` selects `portrait_wraith.tres` before applying generic rarity palettes. This material shares `portrait_palette.gdshader` and maps luminance to shadow RGB `(0.025, 0.004, 0.012)`, middle `(0.48, 0.025, 0.065)`, and light `(0.98, 0.35, 0.32)`, with shadow hold `0.02` and middle point `0.55`. Native undiscovered-card blur and pooled-node reset are preserved.

The native source is `res://images/atlases/card_atlas.sprites/token/soul.tres` in the installed Slay the Spire 2 PCK, observed at 250×190. Runtime references the original resource and applies the shader; no extracted native image is shipped. A 560×245 side-by-side preview rendered both complete portraits at 250×190 confirmed preserved composition with dark burgundy background and readable red spirit/hand silhouettes. The source and preview are optional local observations. `dev.ps1 publish -ExecutionContextLabel escalated` completed with exit code 0 and both existing rig checks passing. A separate engine loaded all four portrait materials, including Wraith, from the published PCK; a temporary native-model observation confirmed Wraith's `PortraitPath` points to Soul. Local game installation and live card-node visual review remain unperformed.
