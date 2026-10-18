# Slay the Spire 2 Art Direction: Official Sources

> Rechecked on 2026-10-18: the V1 URL `steam_screenshot1_new.png` now returns a 2560×1440 Defect combat image, not the Ironclad image described in the earlier observation. Do not use V1 as evidence for the current file. Prefer the [combat-style reinspection](sts2-combat-style-study.md) and [downloaded image/frame index](sts2-style-refs/README.md). "Less painterly" does not mean removing all texture or using exactly three flat colors.

> The V3 URL `steam_screenshot4_new.png` also returned a Silent merchant screen rather than a map in this download. For maps, prefer the `map_qol.jpg` observation in the [future-asset study](sts2-future-asset-reference-study.md). For large selection illustrations, also read the [five-character composition study](sts2-character-select-composition-study.md).

- Research date: 2026-10-18 (Asia/Seoul).
- Purpose: separate the original game's visual language from this mod's production decisions in root `ARTS.md`.
- Scope: Mega Crit development posts, official press kit, distributed screenshots/card images, and the official Steam product page.
- Categories: **Official statement** means written by the developers; **visual observation** means analysis of a directly viewed linked image; **project recommendation** means a proposed application to this mod. Observations and recommendations are not Mega Crit specifications.
- This is research, not a new test, validation asset, or automatic acceptance criterion. Repository asset specifications own implementation/output dimensions.

## 1. Conclusion and Main Correction

**Official statement:** the sequel retains the original's strange, slightly frightening, playful character while becoming clearer, more colorful, and more lively. The developers describe it as less painterly than the first game. [Mega Crit, October 2024](https://www.megacrit.com/news/2024-10-02-neowsletter-issue-3/)

**Project recommendation:** treating "Slay the Spire style" as rough brushwork plus uniform thick black outlines plus muddy colors is inappropriate for the sequel. Preserve reference identity while prioritizing silhouettes, major values, and crisp color boundaries that read in-game. Do not promote rough generation texture into a permanent rule. Face coverage, limb proportions, palette values, and line thickness are project choices.

## 2. Sources and What They Establish

| ID | Primary source | Evidence | Limitation |
|---|---|---|---|
| S1 | [2024-10-02 art direction](https://www.megacrit.com/news/2024-10-02-neowsletter-issue-3/) | First-game readability problems, hiring an Art Director, continuity and changes | Development direction, not final numeric specifications; Community Roundup contains fan art |
| S2 | [2024-11-07 UI update](https://www.megacrit.com/news/2024-11-07-neowsletter-issue-4/) | Rare-card effects, map colors/size, top-bar improvements | Explicitly work in progress; do not copy historical pixel values as current mod specifications |
| S3 | [July 2025 Q&A and cards](https://www.megacrit.com/news/2025-7-16-neowsletter-issue-12/) | Face concealment as a visual choice; actual Colorless cards | URL says July 16, article says July 15; distinguish fan art from game assets |
| S4 | [2025-10-16 Necrobinder reveal](https://www.megacrit.com/news/2025-10-16-neowsletter-issue-15/) | Character/companion roles, cards, presentation | Effects and numbers were in development; analyze presentation only |
| S5 | [2026-03-05 Early Access launch](https://megacrit.com/news/2026-03-05-early-access-launch/) | Launch context, three official screenshots, possible future changes | Distributed screenshots do not necessarily represent every latest asset on 2026-10-18 |
| S6 | [Official press kit](https://www.megacrit.com/press-kits/slay-the-spire-2/) | Trailer links, distributed images, game description | Animated reveals and actual in-game still assets serve different purposes |
| S7 | [Official Steam page](https://store.steampowered.com/app/2868840/Slay_the_Spire_2/) | Mega Crit developer/publisher, Early Access status, game description | User tags are not official art direction |

## 3. Written Production Intent

### 3.1 Continuity and Changes from the First Game

**Official statement:** Casey Yano recalls small humanoid sprites whose pose, grip, and emotion were misread. The sequel needed concise readability at small sizes together with playfulness and expression. The announced goals are Playful, Crisp, Liveliness, Cinematic, and Colorfuler, alongside more animation and full-screen artwork while retaining the original atmosphere. [S1](https://www.megacrit.com/news/2024-10-02-neowsletter-issue-3/)

The brief direct quotation below supplements those original category names:

> “less painterly”

**Project recommendation:** replace vague "dynamic pose" language with explicit hand contact, center of gravity, face direction, and weapon-tip placement. Clarify overlaps before surface texture. Convey darkness through strange subjects and situations, not only low saturation.

### 3.2 Concealed Faces Are Not a Lore Rule

**Official statement:** in the July 2025 Q&A, Casey describes humanoid face concealment as a visual choice supporting player identification, not an in-world reason. [S3](https://www.megacrit.com/news/2025-7-16-neowsletter-issue-12/)

**Project recommendation:** removing Realta's eyes/expression is unnecessary for fidelity. Violet eyes, copper-red hair, and confidence may remain. Use hat shadow and large costume masses to control facial contrast/detail. Necrobinder's dark face belongs to that character and should not be copied.

### 3.3 UI Prioritizes Recognition and Selection

**Official statement:** the November 2024 post cites difficulty tapping small map icons and distinguishing room types on phones as reasons for larger, colored icons. It also describes top-bar spacing/hitbox improvements and sparkling rare-card effects. [S2](https://www.megacrit.com/news/2024-11-07-neowsletter-issue-4/)

**Project recommendation:** design markers and relics through new simplified masses rather than shrinking full-body artwork. Leave rarity, cost, and selection-state presentation to game UI. Do not bake extra numbers, frames, or rarity marks into mod paintings.

## 4. Direct Observations of Game Screens

These are **visual observations** of specific images opened with the web tool, not universal rendering rules. The V1/V3 observations below are historical; their URLs no longer return the described images, as noted above.

### V1. Combat: Different Density for Character, Background, Hand, and Relics

Historical source URL: [official combat PNG](https://www.megacrit.com/images/steam_screenshot1_new.png).

- Red cape/trousers and a light Ironclad mask separated from green/teal stone. Local colors contrasted with the environment without uniform black contours.
- Direction and value separated arm/sword and torso/cape; weapon length and body lean established action.
- Background depth/ornament coexisted with larger value masses near the figure. Environmental depth and character simplification differed.
- The hand fanned across the bottom. Art occupied small upper windows; cost, title, and description occupied separate UI.
- Top-bar relics read as individual objects with less information than character faces, fingers, or folds.
- Health, intent, and status sat near the figure. Painting them into character art would duplicate information.

**Project recommendation:** center standing art on character form, card art on one action, and relics on one object. Check weapon/hat/hem overlap with status UI after integration.

### V2. Selection: Large Illustration Is Bolder than a Combat Sprite

Source: [official Necrobinder selection PNG](https://www.megacrit.com/images/steam_screenshot3_new.png).

- Long curved triangular pink/purple clothes and a huge skeletal hand read before ornament.
- A very dark face selectively reveals eyes/bones, contrasting broad light cloth with deep shadows.
- Many folds turn through angular value planes, with necessary lines and variation added over major shapes.
- Shared violet hues do not imply equal values/saturation; light pink cloth and pale bone create focus.
- Full illustration, left information panel, and bottom portraits have distinct scales and roles.

**Project recommendation:** design a separate selection composition that can emphasize face, hat, and weapon, rather than enlarging a transparent combat sprite. Maintain identity through costume structure, colors, and accessories.

### V3. Map: Textured Parchment and Simple Symbols

Historical source URL: [official map PNG](https://www.megacrit.com/images/steam_screenshot4_new.png).

- Light ochre parchment, rough borders, and faint patterns contrasted with simple clear icons/routes.
- Green, red, and blue rooms also differed in shape, avoiding color-only identification.
- Dotted paths and stamped route marks suggested hand drawing without photoreal leather/paper throughout.
- Dark purple surroundings and separate top/side panels did not justify applying parchment texture to characters.

**Project recommendation:** use one or two marker identifiers such as hat-tip shape or hair color, rather than full-body facial detail and dozens of fur marks.

### V4. Event: Strangeness through Scale, Shape, and Color

Source: [official Neow event PNG](https://www.megacrit.com/images/steam_screenshot5_new.png).

- Neow's large round teal body and bright eyes occupy the left; text/options gather on the right.
- Violet/navy space remains chromatically distinct from the teal creature. Gold details and small bright droplets provide accents.
- Scale differences between creature, entrance, candles, and ornaments make the space strange without obscuring the focal subject.
- Organic variation/shadows leave eyes, mouth, and contour readable rather than covering everything with dense brushwork or metallic shine.

**Project recommendation:** events and selection scenes may use more environment, props, and dramatic scale than combat art while retaining clear focus.

## 5. Cards and Small Objects Viewed Directly

### V5. Thrumming Hatchet: Action Line and Main Object

Source: [official card PNG](https://www.megacrit.com/images/2025_07_hatchet.png), context [S3](https://www.megacrit.com/news/2025-7-16-neowsletter-issue-12/).

**Visual observation:** the hatchet crosses diagonally; blue curves and fragmented light convey motion. Dark background and bright metal establish focus. Wood and metal differ in color/highlight shape. Cost/title belong to the frame.

**Project recommendation:** a large blade/handle/attack-arc relationship can read better than a tiny full-body combat scene for broom-glaive attacks. Keep trails and sparkles from obscuring the weapon.

### V6. Rolling Boulder: Mass from a Limited Subject

Source: [official card PNG](https://www.megacrit.com/images/2025_07_boulder.png), context [S3](https://www.megacrit.com/news/2025-7-16-neowsletter-issue-12/).

**Visual observation:** a large rounded rock, fragments, and dust dominate. Angular planes vary in value without black lines on every crack. Background, dust, and subject directions group into advancing mass.

**Project recommendation:** Guard, armor, and defense cards can focus on one action/material: a dominant subject, supporting effect, and simple background. This is not an official object-count rule.

### V7. Bodyguard / Bone Shards: Related Themes, Different Scenes

Source: [official two-card PNG](https://www.megacrit.com/images/bodyguard_boneshards.png), context [S4](https://www.megacrit.com/news/2025-10-16-neowsletter-issue-15/).

**Visual observation:** one scene emphasizes a broad palm protecting a figure; the other emphasizes radiating bone fragments. Violet clothes and pale bones connect the theme while action silhouettes and backgrounds differ. Pink, teal, and yellow coexist; art is not restricted to its frame color.

**Project recommendation:** do not wash Realta's entire card family in blue. Preserve recurring blue clothes, copper-red hair, and bronze details while selecting accents for Flame, Guard, Spell, or the relevant scene.

### V8. Top Bar: Assets Read with Neighboring Information

Source: [official top-bar JPG](https://www.megacrit.com/images/topbar_qol.jpg), context [S2](https://www.megacrit.com/news/2024-11-07-neowsletter-issue-4/).

**Visual observation:** portrait, health, coins, potions, map, and deck have distinct shapes. Bright internal symbols and dark boundaries separate small icons. Unannounced relics were blurred, so their materials/silhouettes cannot be inferred.

**Project recommendation:** reorganize eyes/hair/hat for portrait recognition and background contrast. A relic such as 청동의 번견 must read as an object's silhouette, not merely as tiny metal scratches.

## 6. Translating Observations into ARTS.md

These are **project recommendations**, not an official production manual.

| Area | Recommended description | Unsupported claim to avoid |
|---|---|---|
| Overall | Slightly eerie, strange fantasy with playfulness/confidence; clear small-scale form/action | Everything must be dark and muddy like horror |
| Lines/edges | Separate color/value/overlap; add dark lines selectively and scale their weight | Always use thick pure-black outlines |
| Painting | Establish major values, then limited gradients/material detail | More rough brush marks mean greater fidelity |
| Materials | Distinguish major reflection shapes and edges of metal/leather/cloth/fur | Use identical cel shading or oil texture everywhere |
| Proportions | Allow simplification/exaggeration for identity and action | All official characters share a fixed head ratio |
| Face | Preserve reference eyes/expression, organized by hat shadow and major values | Eyes/mouth must disappear |
| Color | Separate local/background colors; use accents for role/focus | Every card/effect of a character uses one color |
| Light | Form-defining key light and selective magical light, adapted to scene | Official light is always upper-left with two shading steps |
| Cards | Readable main action/object; cost/text/frame remain UI | Paint the entire card UI into the illustration |
| Animation | Readable preparation/contact/recovery and quick combat flow | A still image establishes official rig specifications |

## 7. Avoiding First-Game, Sequel, and Development-Image Confusion

- **Official statement:** S1 is the direct comparison. A relative reduction in painterliness does not mean no brushwork. [S1](https://www.megacrit.com/news/2024-10-02-neowsletter-issue-3/)
- **Official statement:** some early Silent concepts explored a more cartoon-like direction, not a final norm. [Early concepts](https://www.megacrit.com/images/the_silent_concepts_cropped.png)
- **Visual observation:** those concepts explore elongated skulls, simple bright-green cloaks, and exaggerated eyes. Do not treat their exploratory range as final in-game proportions. [Image](https://www.megacrit.com/images/the_silent_concepts_cropped.png)
- **Official statement:** the press kit distinguishes animated reveal, gameplay, and Early Access trailers. Reveal-film staging is not a combat-sprite specification. [S6](https://www.megacrit.com/press-kits/slay-the-spire-2/)
- **Official statement:** news includes Community Roundup/Community Corner fan art. Hosting on the official site does not make an image an official game asset. [S3](https://www.megacrit.com/news/2025-7-16-neowsletter-issue-12/)

## 8. Research Limits and Follow-Up

1. Public primary sources did not establish universal head ratios, exact HEX palettes, line weights, light angles, shading counts, or rig/layer specifications. Any such ARTS.md values are **project starting points**.
2. The web tool could not play GIFs for Silent, Flail Knight, or some VFX. Animation scope/liveliness was cited from developer prose; this pass did not observe playback or inter-frame motion.
3. Official YouTube gameplay links were found but not played/frame-analyzed. Detailed color/edge/composition observations rely on successfully viewed PNG/JPG files.
4. `screenshot2_new.png` failed to fetch in this initial pass and was not used for its detailed observations.
5. Color analysis was qualitative, without sampling or color-space validation; do not present project HEX values as official colors.
6. Images were public development/launch materials, not a full audit of installed card/relic originals or every latest-patch screen. Steam describes Early Access and planned changes. [S7](https://store.steampowered.com/app/2868840/Slay_the_Spire_2/)
7. This pass was document research. It did not run the game, verify in-game readability, execute automated tests, or change tests/acceptance criteria.

## 9. Reusable Source-Review Procedure

1. Confirm that Mega Crit or the actual creator owns the source.
2. Separate game reveals from fan-art sections.
3. Distinguish written intent from visual interpretation.
4. Record image purpose: combat, selection, card, relic, map, or event.
5. Record date/status: development concept or distributed launch screenshot.
6. Label ARTS.md numbers, palettes, and exclusions as sourced official facts or project recommendations.
7. Have a person review new drafts at final size and within original-game UI. A polished enlarged standalone image does not establish integration suitability.
