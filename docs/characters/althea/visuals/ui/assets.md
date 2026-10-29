# Althea UI Runtime Inventory

## Applied Branch B Delivery (2026-10-24)

| Family | File under `Sts2WD/images/charui/althea/` | Dimensions | Status |
|---|---|---|---|
| Shared standing | `standing.png` | 384×576 RGBA | Applied from the complete adopted base by Fit only; shared by combat, merchant and rest per explicit user instruction |
| Selection illustration | `char_select_bg.png` | 1920×1080 | Applied from the user-selected fully sheathed pre-draw illustration; whole-image Fit, no crop |
| Portrait / outline | `character_icon.png`, `character_icon_outline.png` | 128×128 each | User approved; applied, including `CustomIconOutlineTexturePath` |
| Map marker | `map_marker.png` | 128×128 | User approved; applied |
| Normal / locked tiles | `char_select.png`, `char_select_locked.png` | 132×195 each | User selected high-angle C; applied from shared alpha |
| Energy | `big_energy.png`, `text_energy.png` | 74×74, 24×24 | Existing accepted textures retained unchanged |

## UI Export and Connections

UI export procedure: Fit the square 1254×1254 portrait and marker masters to 128×128. Derive the white 2px dilated portrait silhouette from final alpha. Fit the 1032×1524 high-angle face master to 132×195; composite the same alpha over navy for normal and black over gray for locked. No threshold-based background extraction, cropping or separate locked generation. Existing `CustomIconTexturePath`, select icon paths and `CustomMapMarkerPath` already name the intended runtime files. `CustomIconOutlineTexturePath` now points to `althea/character_icon_outline.png`.

Use [base status](../base/assets.md) for the unchanged source, [standing delivery](../animation/assets.md) for the shared standing export and scene hooks, [selection delivery](../selection/assets.md) for background conversion and the known UI overlap, and [energy delivery](energy-assets.md) for the accepted crimson textures.
