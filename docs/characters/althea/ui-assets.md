# Althea UI Runtime Inventory

## Runtime Inventory

| Family | File under `Sts2WD/images/charui/althea/` | Dimensions | Status |
|---|---|---|---|
| Shared standing | `standing.png` | 384×576 RGBA | Applied from the complete adopted base by Fit only; shared by combat, merchant and rest per explicit user instruction |
| Selection illustration | `char_select_bg.png` | 1920×1080 | Applied from the user-selected fully sheathed pre-draw illustration; whole-image Fit, no crop |
| Portrait / outline | `character_icon.png`, `character_icon_outline.png` | 128×128 each | User approved; applied, including `CustomIconOutlineTexturePath` |
| Map marker | `map_marker.png` | 128×128 | User approved; applied |
| Normal / locked tiles | `char_select.png`, `char_select_locked.png` | 132×195 each | High-angle face composition applied from shared alpha |
| Energy | `big_energy.png`, `text_energy.png` | 74×74, 24×24 | Applied |

## UI Export and Connections

UI export procedure: Fit the square 1254×1254 portrait and marker masters to 128×128, and derive the white 2px dilated portrait silhouette from the final alpha. Fit the 1032×1524 high-angle face master to 132×195, compositing the shared alpha over navy for normal and black over gray for locked, with no threshold-based background extraction, cropping, or separate locked generation. `CustomIconTexturePath`, `CustomIconOutlineTexturePath`, select icon paths, and `CustomMapMarkerPath` point to the runtime files listed above.

Use [base status](base-assets.md) for the visual source, [standing delivery](animation-standing.md#delivery-and-integration) for the shared standing export and scene hooks, [selection delivery](selection-assets.md) for background conversion, and [energy delivery](ui-energy.md#delivery-and-integration) for the crimson textures.
