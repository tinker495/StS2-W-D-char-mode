# Base-Game Character Selection Compositions

- Research date: 2026-10-18 (Asia/Seoul).
- Purpose: make the selection screen convey character and power, beyond placing a full-body figure in a large background.
- Separate **official statements**, **image observations**, and **project judgments**. Observed features are not universal production rules.
- Related: [combat style](sts2-combat-style-study.md), [future assets](sts2-future-asset-reference-study.md).

## 1. Official Material and Inspection Scope

**Official statement:** the press kit distributes separate Early Access, Gameplay, and Animated Reveal trailers. Steam describes five base characters with distinct playstyles and backgrounds. [Press kit](https://www.megacrit.com/press-kits/slay-the-spire-2/), [Steam](https://store.steampowered.com/app/2868840/Slay_the_Spire_2/)

| Material | Source | Treatment in this study |
|---|---|---|
| Necrobinder selection screen | [PNG](https://www.megacrit.com/images/steam_screenshot3_new.png), [launch article](https://megacrit.com/news/2026-03-05-early-access-launch/) | Actual complete screen; character/UI/tile overlaps directly observed. [Saved file](sts2-style-refs/future-assets/ui-select-screenshot.png) |
| Early Access Trailer | [Video](https://www.youtube.com/watch?v=PW22jwFNxU8), [press kit](https://www.megacrit.com/press-kits/slay-the-spire-2/) | Link confirmed; do not infer unviewed frame contents |
| Gameplay Trailer | [Video](https://www.youtube.com/watch?v=ttVtllHkb4E), [press kit](https://www.megacrit.com/press-kits/slay-the-spire-2/) | Distinguish combat footage from selection illustrations |
| Regent reveal | [December 2025 article](https://www.megacrit.com/news/2025-12-11-neowsletter-issue-17/) | Official character/power context; combat-sprite reveals are not selection screens |

Searching official/Steam galleries did not yield other characters' complete selection screenshots. Instead, selection Spine resources from the local base-game installation were rendered into five observation PNGs, all opened directly in this study. Distinguish these resources from public downloads and record their compositing state. Do not invent source URLs or present generated images as original captures.

### Original Selection Illustrations from the Local Game

| Character | Viewed file | Caveat |
|---|---|---|
| Ironclad | [Skeleton render](sts2-style-refs/character-select/ironclad-selection-skeleton.png) | No UI. Red original background elements are present; gray top/bottom margins are observation backing |
| Silent | [Skeleton render](sts2-style-refs/character-select/silent-selection-skeleton.png) | No UI or separate background; gray is not the original background color |
| Defect | [Skeleton render](sts2-style-refs/character-select/defect-selection-skeleton.png) | No UI; gray outer space and lower clipping do not establish finished composition |
| Regent | [Skeleton render](sts2-style-refs/character-select/regent-selection-skeleton.png) | No UI; constellations, color fields, and throne are original; outer gray is backing |
| Necrobinder | [Skeleton render](sts2-style-refs/character-select/necrobinder-selection-skeleton.png) | No UI or separate pink background; compare the complete official screen |

Use these to read pose, arrangement, and major symbols, not as complete game captures or timing verification. [Reference index](sts2-style-refs/character-select/README.md) records resource paths, extraction context, and background hashes. Distribution context: [press kit](https://www.megacrit.com/press-kits/slay-the-spire-2/), [Steam](https://store.steampowered.com/app/2868840/Slay_the_Spire_2/).

## 1.1 Comparison of Five Characters

These are **image observations** of the five original-resource PNGs. UI-free images do not establish actual text overlap.

| Character | Pose, expression, occupancy | Symbol and background | Adopt / do not import |
|---|---|---|---|
| Ironclad | Large mask/shoulders/arms at right foreground; raised sword beside face; lowered head, glowing eyes, tight grips convey threat | Dark red flow and black masses extend heat/danger without requiring a literal castle/cave | Connect face/hands/weapon and extend emotional color; do not copy mask, demonic red forms, or red eyes |
| Silent | Twisted body, one dagger behind and one forward; huge cloak sweeps left; pale mask, green eyes, and blades link focus | Separate background has broad green/blue shifts and long scratched diagonals aligned with movement | Prepared action, sweeping cloth, two or three linked focal points; do not copy skull, dual daggers, or blade-like motifs |
| Defect | Exaggerated claw hand in left foreground; head/chest at right; perspective scale precedes full-leg visibility | Blue field, lightning, luminous chest, and blue rim extend electrical power rather than architecture | Enlarge working hand/object and extend ability light; do not require a mechanical hand or lightning |
| Regent | Reclines in throne, crossed legs, relaxed raised hand; attitude reads through head/shoulder/hand/leg pose despite a small eye | Orange flow and gold constellations connect throne and star face without a literal palace | Convey personality through tension, ease, or work posture; do not substitute throne, constellations, or orange cosmos for a character's own identity |
| Necrobinder | Raised hand and tilted head connect to huge Osty; companion hand becomes the larger subject mass | Pink/violet flow, bone, robe, and flame express strange power together | Two major forms showing character/tool or power relationship; do not copy Osty or skull motifs |

**Project judgment:** the common structure is closer to **one pose, one characteristic major form, and a background extending that power through color/motif** than a full-body figure before a large landscape. More environment detail alone does not create personality. Requiring face, whole hat, entire weapon, and boots in every selection frame prevents bold close cameras. Design such framing during generation; observed off-frame shapes do not authorize later cropping.

Separate layers were also viewed: [Silent's background](sts2-style-refs/character-select/silent-selection-background.png) has broad green/blue shifts and rough diagonals; [Necrobinder's](sts2-style-refs/character-select/necrobinder-selection-background.png) has pink/violet flow and dark curves. Neither layer contains a literal stone fortress/forge set. A separate layer does not establish the absence of additional effects in the finished screen.

## 2. UI Overlap and Left-Panel Safe Space (Necrobinder Screen)

**Image observations** of the complete [official-screen snapshot](sts2-style-refs/future-assets/ui-select-screenshot.png):

- Necrobinder's skull and upper body anchor the right side while giant Osty extends behind the left information panel, matching their shared companion presentation ([October 2025 article](https://www.megacrit.com/news/2025-10-16-neowsletter-issue-15/)).
- Broad bone planes and dark gaps continue behind the left text panel. Safe space works by avoiding **high-frequency detail and strong focal points competing with text**, rather than leaving an empty rectangle.
- Lower character-selection tiles overlay the illustration directly instead of sitting inside blank cutout regions.

**Project judgment:** adopt the relationship between character, second major form, and contrast-managed safe zones—not fixed coordinates ("hero right, giant hand left") or Necrobinder's skull, Osty, and violet-flame motifs. For selection-scene composition rules, safe-zone checks, and production recipes, see [ARTS.md section 5.7](/ARTS.md#57-selection-illustrations) and [the UI asset route](/.agents/skills/character-asset-pipeline/ui.md).
