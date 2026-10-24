# Althea Asset Status

The user approved all five relic candidates on 2026-10-24. All 15 PNGs now occupy Sts2WD/images/relics/ and big/ under their althea_* model filenames. The native Burning Blood path overrides were removed from AltheaRelic; it now inherits the existing Sts2WDRelic path mapping. The complete review overview remains available beside the runtime mapping.


## Applied Relic Artwork (2026-10-24)

The five approved Althea relic illustrations are preserved in `relic-candidates/2026-10-24/` beside this guide. Each slug below has a 94 x 94 `<slug>.png`, a white 2 px alpha-derived `<slug>_outline.png`, and a 256 x 256 `big/<slug>.png`: 15 delivery PNGs total. `overview.png` shows all five whole compositions with 94/48 px light/dark previews. All were generated separately with the built-in image tool. Large exports proportionally fit the whole original canvas; small exports fit it to 90 x 90, centered with 2 px transparent padding in the 94 px canvas before outline generation. No crop or region extraction was used.

| Model | Korean title | Runtime filename slug |
|---|---|---|
| AltheaWyrd | 윌드 | `althea_wyrd` |
| AltheaBloodFrenzy | 피의 광란 | `althea_blood_frenzy` |
| AltheaFightersBracer | 투사의 완갑 | `althea_fighters_bracer` |
| AltheaCrekinMacDubh | 크레킨 막 두브 | `althea_crekin_mac_dubh` |
| AltheaAncestorsRecognition | 선조의 인정 | `althea_ancestors_recognition` |

The user photos establish the sword, bracer and black-cuirass subjects. The accepted crimson energy master supplies the Rare relic's hexagonal rim/circular-center shape with a new blue treatment. The original-game relic/potion reference supplies broad values and contour density only. Physical descriptions and exclusions are recorded in the art guide; optional generation masters/prompts are under `drafts/relic-refresh/`.

All five selections are applied to the runtime paths. AltheaRelic inherits the Sts2WDRelic filename mapping; the old Burning Blood presentation overrides are removed. The installed package and verification record below own deployment status.


## Full-Body Base — Current Status (2026-10-24)

| Item | Status |
|---|---|
| Adopted draft base | `base.png`, explicitly selected by the user as the preferred draft and style anchor. Final design lock and runtime promotion remain open. |
| Appearance | Bright silver-white half-tied long hair in clean flowing locks, calm blue eyes, clean silver plate armor, white split tabard with gold trim and red cross, navy lining, and graceful young-adult proportions. Right-facing three-quarter head and grounded drawn-sword stance; opposite hand supports empty sheath. |
| Selection provenance | Generated with `refs/ref1.jpg` passed directly as an image input alongside official combat references (Defect/Ironclad combat crops and Silent frame 40). The user explicitly confirmed preference for this clean 2D style and rejected the subsequent `style-pure` (compact-proportion / compressed-pewter) experiment as too crude and clunky (`투박함`). |
| Known characteristics | Right sword-tip clearance is `12px` (`1.2%` of width) in the unedited `1024 × 1536` master. Preserve `base.png` unchanged unless a local edit or transparent padding is explicitly requested. |
| File and promotion | `1024 × 1536` RGBA; corner alpha all zero; observed bounds `(67,148)–(1011,1417)`. Byte-for-byte copy of the user-selected candidate, with no crop, padding, resizing, repainting, or alpha cleanup. |
| Review scope | Reviewed via `326 × 490` light/dark whole-image previews and `240 × 360` `IndividualFit` comparison with Defect and Silent. Promotion verified by identical SHA-256 hashes. |
| Runtime | Branch B applies this base to combat/merchant/rest and adds selected UI and selection artwork; see the delivery section. Game-folder installation and gameplay verification remain unperformed. |

The tracked collaboration artifact is `base.png`; appearance and preservation rules are in [althea-art.md](althea-art.md). Optional source and review records remain under `drafts/base-20261024-new/`; they are not required to use the adopted base.

## Energy Icon Draft

The runtime uses the accepted crimson variant at `Sts2WD/images/charui/althea/big_energy.png` and `text_energy.png`. `AltheaCardPool`, `AltheaRelicPool`, and `AltheaPotionPool` point to these shared files. These cover card-cost and inline energy textures. The combat energy counter still uses the placeholder character's native presentation.

The user requested a dark-crimson recolor of the Necrobinder shape and accepted the resulting direction. The generated review master is 1254x1254 RGBA with transparent corners; complete-image Fit conversions are 74x74 RGBA for card costs and 24x24 RGBA for inline energy. The accepted variant is not a pixel-exact hue transform. Runtime replacement and game-folder installation were performed on 2026-10-23. Preserve the complete symbol and alpha; do not crop the painting. Local masters, conversions, and comparisons are optional working material; the tracked runtime textures are the accepted outputs.

To recover the original reference from the installed game, mount `SlayTheSpire2.pck` in Godot, load `res://images/atlases/ui_atlas.sprites/card/energy_necrobinder.tres` as `AtlasTexture`, decompress its atlas image and read its declared region. This recovers the asset's native region, rather than deriving a new composition from a larger painting. The inline source is `res://images/packed/sprite_fonts/necrobinder_energy_icon.png`.

## Red-to-Navy Card Frame Preview

The user requested a red upper frame, subsequently revised to clearer crimson transitioning toward black-blue below. A 598x844 RGBA body and a 1120x620 three-rarity comparison were rendered on 2026-10-22. The shader prototype and comparison preview are optional local working material; the shader procedure is recorded below. The accepted body was copied to `Sts2WD/images/cards/althea/frame.png` on 2026-10-23 and connected through `AltheaCardPool.CustomFrame`. The pool accent color is `#a91f30`. This frame was installed locally on 2026-10-23.

The source body is the native `res://images/atlases/ui_atlas.sprites/card/card_frame_skill_s.tres`, recovered from its decompressed atlas with the declared region and margin. Retain its full canvas and alpha. The draft uses a canvas-item shader with upper RGB `(0.67, 0.12, 0.16)` and lower RGB `(0.065, 0.13, 0.22)`, interpolated by `smoothstep(0.05, 0.9, UV.y)`. Multiply by `clamp(dot(source.rgb, vec3(0.2126, 0.7152, 0.0722)) / 0.29, 0.52, 1.6)` to preserve the native bevels and quiet surface texture. Apply the shader to a standalone texture so UV coordinates cover the whole frame rather than its atlas.

The comparison uses the actual game's `card_banner` and `card_portrait_border_skill_s` textures with the original Common/Uncommon/Rare banner materials and Korean font, plus the crimson energy variant displayed at 64px. The illustration area and effect text are placeholders. This comparison is a resource-based SubViewport preview, not a live `NCard` check. Runtime `RealtaCardAppearance` now also handles Althea: native rarity layers remain visible, body tinting is disabled for the precolored texture, and the type plaque uses `Sts2WD/materials/althea/type_plaque.tres` with `dark_metal.gdshader`. Cost autosizing uses 18–26px with a 3px outline and native positioning. Ancient cards retain their native special layers. Final in-game UI review remains necessary.

## Temporary Portrait Palettes

`Sts2WDCode/Characters/Althea/AltheaPortraitAppearance.cs` applies rarity palettes to the `%Portrait` texture of visible Althea cards after native `NCard.Reload`. It uses `Sts2WD/materials/althea/portrait_common.tres`, `portrait_uncommon.tres`, and `portrait_rare.tres`, sharing `portrait_palette.gdshader`. No native raster or atlas is copied or edited. Existing Attack, Skill, Power, and Curse placeholder sources remain selected by `AltheaCard.Art`.

The shader preserves source alpha and vertex/canvas modulation. It computes luminance with weights `(0.2126, 0.7152, 0.0722)`, raises it to `0.85`, and uses two smooth transitions between shadow, middle, and light colors. The standard transition reaches its middle at `0.55`; Rare holds shadows through `0.22` and reaches gold at `0.65` to retain a black background. Palette RGB values are explicit in the three material files. No randomness, crop, scaling, or composition change is applied at runtime.

Native `NCard.Reload` assigns blur for undiscovered cards; the palette patch leaves non-null materials untouched. Native reload/pool reset clears the palette when a node displays another card. Ancient full-art cards are excluded. Native rarity banners, portrait borders, and frame layers remain independent.

A 840×520 comparison was rendered using complete native Ironclad Strike and Defend portraits at their actual 250×190 size, arranged as two rows and three rarity columns. The comparison checks the same shader/materials used at runtime; it is not a live card-node or gameplay check. Runtime source wiring and packaging are updated, but this portrait palette change has not been installed into the game folder. Earlier card/energy assets were installed on 2026-10-23.

## Wraith Token Portrait

`AltheaCard.Art` selects the native `Soul` model for `AltheaWraith`; all other placeholder source selections remain as above. `AltheaPortraitAppearance` selects `portrait_wraith.tres` before applying generic rarity palettes. This material shares `portrait_palette.gdshader` and maps luminance to shadow RGB `(0.025, 0.004, 0.012)`, middle `(0.48, 0.025, 0.065)`, and light `(0.98, 0.35, 0.32)`, with shadow hold `0.02` and middle point `0.55`. Native undiscovered-card blur and pooled-node reset are preserved.

The native source is `res://images/atlases/card_atlas.sprites/token/soul.tres` in the installed Slay the Spire 2 PCK, observed at 250×190. Runtime references the original resource and applies the shader; no extracted native image is shipped. A 560×245 side-by-side preview rendered both complete portraits at 250×190 confirmed preserved composition with dark burgundy background and readable red spirit/hand silhouettes. The source and preview are optional local observations. `dev.ps1 publish -ExecutionContextLabel escalated` completed with exit code 0 and both existing rig checks passing. A separate engine loaded all four portrait materials, including Wraith, from the published PCK; a temporary native-model observation confirmed Wraith's `PortraitPath` points to Soul. Local game installation and live card-node visual review remain unperformed.

## Local installation and final verification (2026-10-24)

The latest card balance, three new Althea cards, both characters' five relic illustrations, Retain highlighting and Aselige encounter are installed together. Branch: feat/aselige-relics-althea-update, based on origin/main f48f6da. Deployment completed with exit code 0; Sts2WD.dll, Sts2WD.pck, Sts2WD.json and Sts2WD.pdb all matched SHA-256. Destination: Steam/steamapps/common/Slay the Spire 2/mods/Sts2WD. Backup: C:\Users\binib\Desktop\GODOT\StS2-W-D-char-mode\artifacts\runtime\local-install-backup-20261024T121703508Z-38fe34c2a34b4106a0e19d354cba0529. Diagnostics: artifacts/runtime/packaging-20261024T121529526Z-e71ee6b6a0074c31b9c65786d100bba6/.

Temporary native observations passed 51 latest-card cases, 82 Aselige behavior cases and 25 localization/source checks. The installed PCK loaded 72 relic textures, both speaker icons, 34 dialogue lines per language and the complete Ancient painting at three viewport sizes. The C# build had zero warnings/errors. These observations are not formal acceptance tests. No formal tests or validation settings were changed; inherited main changes remain unchanged. The game was not launched and interactive multiplayer was not exercised. The current main workflow intentionally runs animation checks only through dev.ps1 anim; this deployment did not rerun them because no animation was changed here.

## Branch B Delivery — 2026-10-24

| Family | File under `Sts2WD/images/charui/althea/` | Dimensions | Status |
|---|---|---|---|
| Shared standing | `standing.png` | 384×576 RGBA | Applied from the complete adopted base by Fit only; shared by combat, merchant and rest per explicit user instruction |
| Selection illustration | `char_select_bg.png` | 1920×1080 | Applied from the user-selected fully sheathed pre-draw illustration; whole-image Fit, no crop |
| Portrait / outline | `character_icon.png`, `character_icon_outline.png` | 128×128 each | User approved; applied, including `CustomIconOutlineTexturePath` |
| Map marker | `map_marker.png` | 128×128 | User approved; applied |
| Normal / locked tiles | `char_select.png`, `char_select_locked.png` | 132×195 each | User selected high-angle C; applied from shared alpha |
| Energy | `big_energy.png`, `text_energy.png` | 74×74, 24×24 | Existing accepted textures retained unchanged |

### Conversion and Connection Contract

`base.png` stays 1024×1536, SHA-256 `51DB2A49CD46AC74BF97F7BF692BE6A00300B8BD1D9FA956C5973B5D3F144B22`. `standing.png` is a complete-image Fit to 384×576; no added padding or crop. Do not ship a separately generated combat painting. Combat and merchant show this texture at scale 0.625 (240×360 canvas); rest uses 0.85 (326.4×489.6). Canvas scale is not opaque subject height. The base's narrow sword-tip margin remains intentional preservation.

The selected illustration master is 1672×941 RGB. Whole-image Fit into 1920×1080 preserves the complete painting, with the approximately 1px Fit margin and resampling edge backed by opaque navy `#101321`; no image pixels are cropped. The runtime selection `TextureRect` uses `KEEP_ASPECT_CENTERED` (`stretch_mode = 5`), not Cover. The sheathed sword extends toward the lower-right illustration boundary; no post-generation crop is performed.

`Althea.CustomVisualPath`, `CustomMerchantAnimPath`, `CustomRestSiteAnimPath` and `CustomCharacterSelectBg` point to `Sts2WD/scenes/althea/combat.tscn`, `merchant.tscn`, `rest_site.tscn` and `char_select_bg_althea.tscn`. Combat exposes `Visuals`, `Bounds`, `CenterPos`, `OrbPos`, `TalkPos` and `FormVfx`; merchant exposes `Visuals`; rest exposes `ControlRoot/Visuals`, `Hitbox` and thought-bubble anchors. These are static sprites, with no new animation or event bridge. Character selection uses a full-rect Control/TextureRect.

UI export procedure: Fit the square 1254×1254 portrait and marker masters to 128×128. Derive the white 2px dilated portrait silhouette from final alpha. Fit the 1032×1524 high-angle face master to 132×195; composite the same alpha over navy for normal and black over gray for locked. No threshold-based background extraction, cropping or separate locked generation. Existing `CustomIconTexturePath`, select icon paths and `CustomMapMarkerPath` already name the intended runtime files. `CustomIconOutlineTexturePath` now points to `althea/character_icon_outline.png`.

### Selection Revision — Approved and Applied

The user selected high-angle candidate C and the new fully sheathed pre-draw background after the UI overlap was reported. The tile preserves a concentrated upward gaze under lowered brows. The background shows a lowered left-facing head, hilt hand, bracing hand and curse currents gathering at the scabbard throat. It replaces the earlier drawn-sword painting. The normal/locked pair shares the same complete-image Fit alpha; the background uses whole-image Fit with an opaque navy backing. Current runtime files, not ignored drafts, are the shared visual source of truth.

Full-image previews and a 132×195 comparison with native Silent/Defect tiles were opened. An approximate selection UI composite keeps the face and both hands clear; the ascension panel overlaps the bottom of the crossguard. This known composition tradeoff was disclosed before the user requested application. It is not a live-game observation. Optional local masters are under `drafts/select-revision-20261024/`.

Portrait, marker, energy, shared standing and scene wiring remain as previously approved. The base hash remains unchanged. Runtime selection textures are 132×195, 132×195 and 1920×1080, all opaque. Earlier publish attempts reported MSBuild Exec -1 after savepack and succeeded outside the sandbox. The logs contain `EXEC : error : Failed to read the root certificate store.`; this was output parsing, not evidence of an engine crash. After separating that environmental diagnostic from real errors and requiring savepack completion, one sandbox publish completed successfully without escalation or retry. A separate clean engine mounted the resulting PCK, loaded all nine Althea textures and four scenes, and observed the selection dimensions and Fit mode 5; it exited 0. Local game installation and live gameplay verification were not performed. No tests or acceptance assets were added or changed.
