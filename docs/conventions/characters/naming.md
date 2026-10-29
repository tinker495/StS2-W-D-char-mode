# Character Naming and Layout

Use the character's decided English name for owned paths and identifiers. `<Char>` denotes that type prefix and `<char>` its lowercase storage name; Realta/realta and Althea/althea are examples, not a closed list. The third character's approved name is Mercurius/mercurius. For undecided names, retain the existing `CharacterN` / `character_n` identifiers until a name is approved. Resolve the actual model type and resource paths before choosing a documentation directory; a display label, filename or legacy `CharacterId` constant alone does not authorize a rename.

| Area | Playable-character layout |
|---|---|
| Code | `Sts2WDCode/Characters/<Char>/`, namespace `Sts2WD.Sts2WDCode.Characters.<Char>`, with `Cards/`, `Relics/`, `Potions/`, `Powers/` |
| Types | Model `<Char>`; pools `<Char>CardPool`, `<Char>RelicPool`, `<Char>PotionPool`; pool bases `<Char>Card`, `<Char>Relic`, `<Char>Potion` carry `[Pool]`; every other owned type is `<Char>`-prefixed. One top-level type per file, named after that type: each card, relic, potion and power model has its own `<Type>.cs` and `.cs.uid`; keep a class in one file, without `partial` splits |
| Assets | `Sts2WD/images/charui/<char>/`, `Sts2WD/images/cards/<char>/`, `Sts2WD/materials/<char>/`, `Sts2WD/character_text/<char>.json`; resolve additional portrait/icon locations through the owner's asset guide |
| Docs | `docs/characters/<char>/`: design `<char>.md`; short `<char>-art.md` and `<char>-assets.md` indexes; `<char>-history.md`; topic documents under `visuals/<topic>/` as defined by [character guides](../documentation/character-guides.md) |

Ancients and other visual owners retain their native role and existing code/resource roots. For example, Aselige uses `Sts2WDCode/Ancients/Aselige/` and ancient-event resources; sharing a documentation layout does not make it a playable `CharacterModel` or move it into `Characters/`.

## Rename Compatibility

Model IDs are `STS2WD-` plus the UPPER_SNAKE class name. Renaming a model class changes its saved ID; moving folders or namespaces leaves IDs untouched. A model rename also carries the `STS2WD-<CHAR>.*` keys in `Sts2WD/localization/*/characters.json` and `ancients.json`, and the preset's `characterId`.

Moving a texture rewrites its `.import` `source_file` and `.ctex` hash, the MD5 of the `res://` source path. Record save-compatibility outcomes in the character's design document. A documentation reorganization alone does not authorize model, namespace, texture or save-ID migration.
