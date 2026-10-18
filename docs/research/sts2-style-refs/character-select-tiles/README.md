# Original Base-Game Character Selection Tiles

- Research date:2026-10-18 (Asia/Seoul).
- Inspected installation: **v0.107.1**, commit **59260271**, from local game metadata.
- Detailed analysis: [selection-tile study](../../sts2-character-select-tile-study.md).
- Source: Mega Crit's local base-game resource pack. Distribution context: [press kit](https://www.megacrit.com/press-kits/slay-the-spire-2/), [official Steam page](https://store.steampowered.com/app/2868840/Slay_the_Spire_2/).
- Extraction: load each `Texture2D` and call `get_image().save_png()`. No resizing, background removal, color correction, or repainting. These are neither generated images nor composited button screenshots.
- Inspection: all five normal and five locked images were opened directly. Every file is a **132×195 PNG**.

## Original Resource Paths

| Character | Normal original | Locked original |
| --- | --- | --- |
| Ironclad | [ironclad-char-select.png](ironclad-char-select.png) — `res://images/packed/character_select/char_select_ironclad.png` | [ironclad-char-select-locked.png](ironclad-char-select-locked.png) — `res://images/packed/character_select/char_select_ironclad_locked.png` |
| Silent | [silent-char-select.png](silent-char-select.png) — `res://images/packed/character_select/char_select_silent.png` | [silent-char-select-locked.png](silent-char-select-locked.png) — `res://images/packed/character_select/char_select_silent_locked.png` |
| Defect | [defect-char-select.png](defect-char-select.png) — `res://images/packed/character_select/char_select_defect.png` | [defect-char-select-locked.png](defect-char-select-locked.png) — `res://images/packed/character_select/char_select_defect_locked.png` |
| Regent | [regent-char-select.png](regent-char-select.png) — `res://images/packed/character_select/char_select_regent.png` | [regent-char-select-locked.png](regent-char-select-locked.png) — `res://images/packed/character_select/char_select_regent_locked.png` |
| Necrobinder | [necrobinder-char-select.png](necrobinder-char-select.png) — `res://images/packed/character_select/char_select_necrobinder.png` | [necrobinder-char-select-locked.png](necrobinder-char-select-locked.png) — `res://images/packed/character_select/char_select_necrobinder_locked.png` |

The resource paths and extraction method above identify the observations without local logs or scripts. These files are not new product tests or acceptance criteria.

## Snapshot Identification

SHA-256:

| File | Hash |
| --- | --- |
| `ironclad-char-select.png` | `27980341F7574E0CD8ABDCF9F89BD88A9EB92650C0669AC060C44F40ABE66D0B` |
| `ironclad-char-select-locked.png` | `EEFFCC9984CC25075F4CC6025E31C3968A85F49D61A8A7A1575645A50F241B62` |
| `silent-char-select.png` | `4464E2B87AECA9AEF08936E5F78AAB33DF38417300B6D10CC825AE918132C8FF` |
| `silent-char-select-locked.png` | `CEF1F5F505AFF18882E38101DE5A8E3F544F2ACC2AF6F6BD2DE715ED0592B16C` |
| `defect-char-select.png` | `D57495CBF704833F624F40BD44B3DF99C9B80B65206759CBB58BA8B8C03FB919` |
| `defect-char-select-locked.png` | `D9619766CA4ADBA849D9966310A8196DAE626D55734C3C0111721AED880CDB28` |
| `regent-char-select.png` | `658D034211F9037999B6A4EE73691C9371A0F8957DC329D0CA6CA5E566AE78DB` |
| `regent-char-select-locked.png` | `9AD055C51FFCDE04D664A1E94A45267D6262F64D85B2B464BE85644CDC1404C5` |
| `necrobinder-char-select.png` | `2EBBBE951C05320A4302AB0BFEB8FD6CC98F0097B5EF287C0FE2AC213C41EA3D` |
| `necrobinder-char-select-locked.png` | `945F172E13ABA598288F0CA4BACE23EA07FC0AE62E32CCDF1B14B1C6F049B048` |

## Distinct States

- **Normal source tile:** colored art centered on the face/mask.
- **Deselected tile in a screen:** source art with game deselection treatment, visible in the [official screen crop](../future-assets/ui-select-tiles-crop.png).
- **Locked source tile:** black silhouette on gray, distinct from merely darkening colored art.

Distinguish thin color lines in source PNGs from game selection frames, masks, and focus effects. These references do not instruct painting gold borders, lock icons, or button corners into Realta's master.

## Application to Realta Production

Use original camera distance, face occupancy, and expression to **generate new selection-tile art directly**. Do not default to extracting/enlarging full-body or waist-up rectangles or discarding surroundings with Cover. Fit-resize the complete generated master and share its alpha for normal/locked compositing. Revise generation composition when face occupancy is wrong.

Clipped horns/headwear in originals are observed composition choices. Use them to design intentional frame boundaries during generation, not to authorize post-generation cropping. Distinguish research screenshot crops from asset production. This is an explicit project workflow, not verified knowledge of Mega Crit's internal process.
