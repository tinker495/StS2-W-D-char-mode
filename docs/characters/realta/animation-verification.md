# Realta Animation and UI Observation Scope

[Asset index](realta-assets.md).

## Combat Expressions

The combat and merchant rigs use `head-closed.png`, `head-hurt.png`, and `head-attack.png` under `Sts2WD/animations/realta/combat/parts/`. The textures share the 188×126 RGBA rectangle at source offset `(140,99)` with `head.png`. A connected facial-skin mask preserves the silhouette, hair, hat, and jaw junction across the texture swaps. Use [animation sources](animation-sources.md) for the tracked coordinate manifests and bake procedure.

## Verification Scope

The tracked [rig checks](/tests/rig-checks.md) verify scene structure, dispatch, and authored motion. [Scene connections](animation-assets.md#current-scene-and-property-connections) identifies how the game selects each resource. Installation and play observations belong in the [deployment record](/docs/deployment-status.md).
