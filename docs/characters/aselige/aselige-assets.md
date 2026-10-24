# Aselige Asset Status

## Current runtime application (2026-10-24)

The approved refined forest main painting, matching cute dialogue head and all 14 relic icons are now connected to the implemented Ancient. The full Korean title is **정록 용, 아셀리게**; the English title is **Verdant Dragon, Aselige**. Both epithet entries remain empty to prevent duplication.

| Role | Path | Dimensions / wiring |
|---|---|---|
| Approved visual anchor | docs/characters/aselige/base.png | 1875 x 839, opaque PNG |
| Event texture | Sts2WD/images/ancients/aselige.png | Byte-identical to base |
| Event scene | Sts2WD/scenes/ancients/aselige.tscn | Script-free full-rect Control; whole-painting Keep Aspect Centered; dark forest ColorRect; mouse input ignored |
| Dialogue/history/map icon | Sts2WD/images/ancients/aselige_icon.png | 85 x 85 RGBA, whole-head Fit |
| Icon outline | Sts2WD/images/ancients/aselige_icon_outline.png | 85 x 85, white 1 px alpha-derived outline |
| Relic small / outline | Sts2WD/images/relics/aselige_<slug>.png / aselige_<slug>_outline.png | 94 x 94; outlines use white 2 px alpha dilation |
| Relic large | Sts2WD/images/relics/big/aselige_<slug>.png | 256 x 256 |

Main/base SHA-256: 5C65B906DB8552E604C34EF774D6CA9B0C8BA8B16111194C47C619D2B122F5E8. Promotion copied the approved painting unchanged. The 85 px head and outline were copied from the reviewed complete-painting exports; no crop, extraction, repaint or automatic Cover fitting was performed. The original miniature master is optional production history, not a runtime dependency.

## Relic filename mapping

The fourteen slugs are regeneration_grace, deep_breath, harmonious_blessing, life_source, embracing_flute, shining_film, purifying_breath, deep_blue_scales, clean_pebble, small_sprout, large_sapling, pretty_starfish, neatly_arranged_salt and growing_blessing. Each has the three exports listed above, totaling 42 textures. The current overview is relics-overview.png.

Concrete model names are Aselige followed by the PascalCase slug. Sts2WDRelic resolves their filenames from the model entry without the STS2WD- prefix. The internal gift enum retains DragonFlute; the concrete model and files use EmbracingFlute.

## Runtime integration

Sts2WDCode/Ancients/Aselige/Aselige.cs derives from BaseLib 3.4.7 CustomAncientModel, entry STS2WD-ASELIGE, and explicitly supplies the scene, map and history icon paths. AseligeHooks replaces the first Act's generated Ancient for any party containing a mod character. Native-only parties and later Acts retain their generated Ancients. NAncientDialogueLine uses the history icon/outline for the speaker; NAncientNameBanner reads the complete title and empty epithet.

AseligeRelics owns the 14 concrete effects and save properties. The two granted cards are implemented as zero-cost Skills. AseligeDialogues selects the authored 13 conversations per visitor and first Act, without caching run-specific text on the canonical model. Korean and English runtime localization includes relics, cards, event descriptions and all dialogue lines/continuation buttons.

## Verification scope

Temporary native headless observations cover 82 cases: command hooks, +9 Leaf energy, permanent card grants, relic state save round trips, branching map insertion/save restoration, actual native room generation, 13 dialogue populations, event options, potion-use max HP, relic obtain notifications, Common-only elite enchantments and alternating extra card rewards. The earlier rules observation covered 320 route/weight/dialogue combinations. These are diagnostic observations, not formal acceptance tests. Formal repository tests and CI assets were not modified.

The earlier artwork-only `dev.ps1 publish` completed with exit code 0 on 2026-10-24, on `feat/althea-cards` at 4130792 plus the uncommitted Aselige implementation. The C# build has zero errors and warnings; existing Realta rig checks passed 41,864 native samples and instance isolation passed 4 cases. A temporary resource observation loaded all 42 relic textures, both 85 px speaker icons, 34 dialogue lines in each language and the complete 1875 x 839 main painting, and checked layout at 1920 x 1080, 1280 x 800 and 2560 x 1080.

Output: `artifacts/mods/Sts2WD/`. Successful diagnostics: `artifacts/runtime/packaging-20261024T023216259Z-e3b90af334dc4136a2fc2cc49d059115/`. The preceding sandbox export failed with certificate-store/access errors; the elevated retry succeeded. This operational recovery does not establish a permanent environment fix. That earlier artwork-only run did not replace the local Steam installation or launch the game. The latest full installation is recorded separately below. Headless observations do not establish multiplayer network or interactive presentation correctness. Earlier production records are in aselige-history.md.

## Local installation and final verification (2026-10-24)

The latest card balance, three new Althea cards, both characters' five relic illustrations, Retain highlighting and Aselige encounter are installed together. Branch: feat/aselige-relics-althea-update, based on origin/main f48f6da. Deployment completed with exit code 0; Sts2WD.dll, Sts2WD.pck, Sts2WD.json and Sts2WD.pdb all matched SHA-256. Destination: Steam/steamapps/common/Slay the Spire 2/mods/Sts2WD. Backup: C:\Users\binib\Desktop\GODOT\StS2-W-D-char-mode\artifacts\runtime\local-install-backup-20261024T121703508Z-38fe34c2a34b4106a0e19d354cba0529. Diagnostics: artifacts/runtime/packaging-20261024T121529526Z-e71ee6b6a0074c31b9c65786d100bba6/.

Temporary native observations passed 51 latest-card cases, 82 Aselige behavior cases and 25 localization/source checks. The installed PCK loaded 72 relic textures, both speaker icons, 34 dialogue lines per language and the complete Ancient painting at three viewport sizes. The C# build had zero warnings/errors. These observations are not formal acceptance tests. No formal tests or validation settings were changed; inherited main changes remain unchanged. The game was not launched and interactive multiplayer was not exercised. The current main workflow intentionally runs animation checks only through dev.ps1 anim; this deployment did not rerun them because no animation was changed here.