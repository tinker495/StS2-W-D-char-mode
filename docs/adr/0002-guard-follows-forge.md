# Guard Follows Forge's Creation, Tracking, and Growth Rules

Guard (수호) creates a card during combat and accumulates its value, following Regent's Forge (`ForgeCmd` and `SovereignBlade`), instead of storing Block in a dedicated power. Use Forge's handling of full hands, exhaustion, and copies as the baseline. Reusing established rules avoids inventing each edge case and preserves the behavior players learned with Regent.

## Consequences

Inspection on 2026-10-16 established these Forge rules; Guard follows them:

- Create a new Guard card in hand only when no Guard card exists outside the exhaust pile. With a full hand, normal generated-card handling (`CardPileCmd`) sends it to the discard pile.
- Apply growth to every Guard card, including exhausted cards. Gaining Guard after its card is exhausted therefore creates a new card and also increases the exhausted card's value.
- Exclude only temporary duplicates (`IsDupe`, copies sent to Limbo after use) from tracking and growth. Ordinary copies created by other effects count as Guard cards and grow together. "One per combat" describes the condition for Guard to create a new card.
- The user's 2026-10-19 balance revision makes the token Legacy: cost stays at 1 and each upgrade adds 3 Block. This explicitly replaces Sovereign Blade's cost-reduction upgrade. Creation still starts at zero before applying the gained Guard amount; the sheet's 5 describes the starting relic's first token, not an additional base value. Reverting upgrades preserves Guard growth but removes upgrade-added Block.
- Guard cards are generated during combat and disappear afterward; their accumulated values do not persist in the deck.
- Self Repair retrieves existing Guard cards before applying Guard. The user reported on 2026-10-19 that applying Guard first created a new card when the original was exhausted, then retrieved the original as well. Retrieval-first preserves Forge's general creation rule while avoiding this unwanted duplication. With no existing Guard, Self Repair still creates one through the normal Guard command; a full-hand retrieval moves the existing card to discard, where Guard can find it.

If this list conflicts with the design document, or an edge case has no Forge precedent, request a user decision for that case.
