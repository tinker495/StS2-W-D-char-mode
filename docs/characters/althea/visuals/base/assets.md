# Althea Draft Base Status

| Item | Status |
|---|---|
| Adopted draft base | `base.png`, explicitly selected by the user as the preferred draft and style anchor. Final design lock remains open; the separate approved runtime reuse is recorded in [standing delivery](../animation/assets.md). |
| Appearance | Bright silver-white half-tied long hair in clean flowing locks, calm blue eyes, clean silver plate armor, white split tabard with gold trim and red cross, navy lining, and graceful young-adult proportions. Right-facing three-quarter head and grounded drawn-sword stance; opposite hand supports empty sheath. |
| Selection provenance | Generated with `refs/ref1.jpg` passed directly as an image input alongside official combat references (Defect/Ironclad combat crops and Silent frame 40). The user explicitly confirmed preference for this clean 2D style and rejected the subsequent `style-pure` (compact-proportion / compressed-pewter) experiment as too crude and clunky (`투박함`). |
| Known characteristics | Right sword-tip clearance is `12px` (`1.2%` of width) in the unedited `1024 × 1536` master. Preserve `base.png` unchanged unless a local edit or transparent padding is explicitly requested. |
| File and promotion | `1024 × 1536` RGBA; corner alpha all zero; observed bounds `(67,148)–(1011,1417)`. Byte-for-byte copy of the user-selected candidate, with no crop, padding, resizing, repainting, or alpha cleanup. |
| Review scope | Reviewed via `326 × 490` light/dark whole-image previews and `240 × 360` `IndividualFit` comparison with Defect and Silent. Promotion verified by identical SHA-256 hashes. |
| Runtime | Branch B applies this base to combat/merchant/rest and adds selected UI and selection artwork; see [standing delivery](../animation/assets.md) and the [UI inventory](../ui/assets.md). Game-folder installation and gameplay verification remain unperformed. |

The tracked collaboration artifact is [base.png](../../base.png); appearance and preservation rules are in [appearance and preservation](appearance.md). The selection provenance and review observations above describe the adopted base without requiring local production records.
