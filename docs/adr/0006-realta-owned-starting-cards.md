# Realta Owns Its Strike and Defend Models

Define Realta's starting Strike and Defend as `RealtaStrike` and `RealtaDefend`, registered in `RealtaCardPool`. In `sts2.dll` (SHA256 `A1F9E653F1E28E4076558FEE1E60D218619CB7E057B887C6417F62C62C6D7A52`), `CardFactory.GetDefaultTransformationOptions` resolves candidates from `original.Pool` rather than `CharacterModel.CardPool`, and `MorphicGrove.Group` → `CardCmd.TransformToRandom` → `CardTransformation.GetReplacement`, `NTransformPreview`, `Astrolabe`, and `PandorasBox` all share this default API. Sharing Ironclad models while patching transformation methods misaligns ownership and candidate semantics and couples the mod to external method implementations.

Preserve the original costs, values, upgrades, and Strike/Defend tags. Existing placeholder art may be reused. New model IDs are part of save compatibility; consider migration in any later rename.

For existing Realta runs, migrate only `StrikeIronclad` and `DefendIronclad` IDs in the deck during player restoration, preserving saved card properties. Old data cannot distinguish where cards sharing these models were acquired, so migrate both IDs wherever present in that deck. Do not migrate other characters' decks or historical run records. Ordinary random transformations and transformations with explicit results use the game's original paths.
