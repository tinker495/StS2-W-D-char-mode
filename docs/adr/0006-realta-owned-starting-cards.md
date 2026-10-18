# Realta Owns Its Strike and Defend Models

Define Realta's starting Strike and Defend as `RealtaStrike` and `RealtaDefend`, registered in `RealtaCardPool`. Default transformations use the card's own pool. Sharing Ironclad models while patching transformation methods misaligns ownership and candidate semantics and couples the mod to external method implementations. On 2026-10-18, the user chose dedicated starting cards to correct ownership.

Preserve the original costs, values, upgrades, and Strike/Defend tags. Existing placeholder art may be reused. New model IDs are part of save compatibility; consider migration in any later rename.

For existing Realta runs, migrate only `StrikeIronclad` and `DefendIronclad` IDs in the deck during player restoration, preserving saved card properties. Old data cannot distinguish where cards sharing these models were acquired, so migrate both IDs wherever present in that deck. Do not migrate other characters' decks or historical run records. Ordinary random transformations and transformations with explicit results use the game's original paths.
