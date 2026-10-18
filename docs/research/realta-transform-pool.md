# Realta Starting-Card Transformation Pools

Investigation date: **2026-10-18, Asia/Seoul**. Transforming Realta's starting Strike and Defend in Morphic Grove produced Ironclad cards. The agreed fix was to give Realta its own starting-card models.

## Observed Base-Game Behavior

The inspected installation's `sts2.dll` SHA256 was `A1F9E653F1E28E4076558FEE1E60D218619CB7E057B887C6417F62C62C6D7A52`. The following observations identify assembly types and members rather than requiring a private decompilation cache.

- The old starting deck used four `StrikeIronclad` and four `DefendIronclad`. Although the character's `CardPool` was `RealtaCardPool`, `CardModel.Pool` for those cards was `IroncladCardPool`. [Previous starting deck](https://github.com/tinker495/StS2-W-D-char-mode/blob/71a4f05982271d819754dbf0d39f678d22ca4293/Sts2WDCode/Characters/Realta/Realta.cs).
- `MorphicGrove.Group` selects two cards and calls `CardCmd.TransformToRandom` for each. `CardTransformation.GetReplacement` calls the default factory when neither an explicit replacement nor candidates are supplied.
- `CardFactory.GetDefaultTransformationOptions` gets candidates from `original.Pool`, not the owner's character pool. Quest, Event, Ancient, and Token cards use the Colorless pool. It applies unlock and multiplayer filters, allows Common/Uncommon/Rare for ordinary transformation, and excludes the same ID and combat-ineligible cards under the relevant conditions. `CardPoolModel` owns the unlock/multiplayer filtering.
- `NTransformPreview`, `Astrolabe`, and `PandorasBox` use the same default candidate API. Correcting starting-card ownership therefore avoids per-event patches.

**Observed before the fix:** two runs with the same seed reported character `Realta` and 66 unlocked cards in its pool, but 80 Ironclad candidates for Strike and Defend. `CardTransformation.GetReplacement` actually returned `Dark Embrace`; the temporary observation reported two failures. Character registration and an empty reward pool were not the cause.

## Implementation and Save Compatibility

`RealtaStrike` and `RealtaDefend` are concrete `RealtaCard` models, with four of each in the starting deck. They inherit `[Pool(typeof(RealtaCardPool))]`. Both are Basic, so they are excluded from ordinary rewards and random transformation results. Strike costs 1 and deals 6 damage (9 upgraded); Defend costs 1 and grants 5 Block (8 upgraded). Preserve Strike/Defend tags and `GainsBlock`; only placeholder art is reused. [Starting deck](../../Sts2WDCode/Characters/Realta/Realta.cs), [Strike](../../Sts2WDCode/Characters/Realta/Cards/RealtaStrike.cs), [Defend](../../Sts2WDCode/Characters/Realta/Cards/RealtaDefend.cs), [pool registration](../../Sts2WDCode/Characters/Realta/Cards/RealtaCard.cs).

Game transformation methods are not patched. Other-character cards acquired by Realta, Colorless/Curse/Status cards, and transformations with explicit results or candidates follow the original rules. The ownership decision is recorded in [ADR-0006](../adr/0006-realta-owned-starting-cards.md).

Before `Player.FromSerializable`, replace `StrikeIronclad` and `DefendIronclad` IDs only in Realta's saved deck. Preserve the same `SerializableCard` upgrade count, enchantments, properties, and acquisition floor, then use normal restoration. Other characters' saves are unchanged. Old Realta saves cannot distinguish the acquisition source of cards sharing those models, so all occurrences of those two IDs remaining in the deck are migrated. [Migration implementation](../../Sts2WDCode/Characters/Realta/RealtaStartingDeckMigration.cs).

## Verification Performed During the Investigation

Temporary observation code ran the matching installed game assembly in a separate headless process and profile with BaseLib and the built mod. It did not write to the user's installed game or saves. No formal tests or validation assets were created or changed. The following records summarize those observations without requiring the local runner or logs; they are not new acceptance criteria.

- All 10 starting cards belonged to Realta's pool, including four dedicated Strikes and four dedicated Defends. Both cards had 62 Realta transformation candidates and returned Realta cards.
- Base/upgraded values, Strike/Defend tags, Korean names, and placeholder portrait loading were checked. Actual combat observations showed Strike damage 6/9, Defend Block 5/8, energy cost 1, and movement to discard after each use.
- Observations covered upgraded starters, combat candidates, ordinary Realta cards, acquired `Anger`, Colorless/Curse/Status cards, fixed results, and explicit candidate transformations.
- Restoring old IDs through `Player.FromSerializable` produced the new cards while preserving upgrades, Sharp 2, and acquisition floor. Reloading new saves was idempotent; Ironclad saves were not migrated.
- In a two-player `RunState`, Realta and Ironclad starters used their respective pools and excluded singleplayer-only candidates. This did not verify network synchronization.
- Running `MorphicGrove.BeginEvent` and the `GROUP` option with an automatic chooser selected one Strike and one Defend. Gold changed 100→0, deck size stayed 10, and the event completed. Replacements were `RealtaSteelPincers` and `RealtaFathersAdvice`.

The final temporary observation reported zero failures. `dev.ps1 build` completed with zero warnings and errors. The first sandboxed `dev.ps1 publish` failed on Windows certificate-store access; a local packaging retry with expanded permissions succeeded.

Not performed: visual inspection of the event UI or preview, individual Astrolabe/PandorasBox execution, resuming an actual old save file, or network multiplayer synchronization. The result was not deployed to the local game installation.
