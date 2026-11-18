# Althea Base Asset Status

| Item | Status |
|---|---|
| Adopted base | [base.png](base.png), the current visual anchor and source of the shared runtime standing sprite. See [standing delivery](animation-standing.md#delivery-and-integration) for runtime reuse. |
| Appearance | Bright silver-white half-tied long hair in clean flowing locks, calm blue eyes, clean silver plate armor, white split tabard with gold trim and red cross, navy lining, and graceful young-adult proportions. Right-facing three-quarter head and grounded drawn-sword stance; opposite hand supports empty sheath. |
| References | [Bust painting](refs/ref1.jpg) for identity. Native Defect, Ironclad, and Silent combat art for staging and rendering. |
| Known characteristics | Right sword-tip clearance is `12px` (`1.2%` of width) in the unedited `1024 × 1536` master. Preserve `base.png` unchanged unless a local edit or transparent padding is explicitly requested. |
| File | `1024 × 1536` RGBA with transparent corners. Inclusive alpha bounds are `(67,148)–(1011,1417)`. |
| Runtime | Combat, merchant, and rest use a whole-image Fit export of this base. See [standing delivery](animation-standing.md#delivery-and-integration) and the [UI inventory](ui-assets.md) for current mappings. |

See [appearance and preservation](base-appearance.md) for the current visual specification and open design decisions.
