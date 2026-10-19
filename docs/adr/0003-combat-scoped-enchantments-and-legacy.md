# Realta's Enchantments Are Combat-Scoped; Only Legacy Allows Multiple Enchantments

Base-game enchantments permanently attach to deck cards and occupy one slot per card. Realta grows cards during combat, so effects such as Refine grant enchantments for that combat by default. Permanent application is reserved for the Ancient card 천재의 영감. Ordinary cards keep the base game's one-enchantment restriction: an enchanted card cannot receive another application or replacement, even of the same type. Only cards with Legacy (유산) may receive repeated applications or multiple enchantment types.

## Considered Options

- **Permanent application by default.** Consistent with the base game, but a starting deck that repeatedly uses Refine would complete its progression too quickly. Rejected.
- **Multiple enchantments on every card.** Removes Legacy's identity and requires reconsidering interactions across existing cards and enchantments. Rejected.

## Consequences

- Combat cards are copies of deck cards (`CardModel.DeckVersion`). Enchantments applied to combat copies disappear with them after combat; permanent enchantments on deck cards remain unchanged.
- Keep temporary and permanent enchantment amounts separate, even for the same type. The user's 2026-10-19 card-sheet feedback replaces the original named temporary entries: show temporary effects in aqua, omit temporary enchantment titles and amounts, and retain permanent entries. Damage and Block values changed by temporary enchantments are aqua unless upgrade previews or combat modifiers require the game's normal highlight colors. Combine stored amounts only when type and lifetime both match.
- Base-game cards have one enchantment slot, and `Sharp` and `Adroit` cannot stack even with their own type (`IsStackable == false`). Use `RealtaCompositeEnchantment` in the slot when one permanent enchantment cannot represent the state: multiple Legacy enchantments or any temporary enchantment.
  - The composite stores original enchantment instances and delegates damage, Block, replay counts, on-play effects, and hooks to its entries. Base-game code continues reading the slot.
  - Card-level rules in `RealtaEnchantments` own the Legacy exception. It applies to permanent base-game applications from events and relics as well as Refine. Ordinary cards with an occupied slot cannot be enchanted again through any path.
  - Permanent entries are serialized; temporary entries exist only on combat cards.
  - Base-game effects that transfer a whole slot enchantment to another card, such as Claws' Maul transformation, do not transfer the composite itself. Reapply permanent entries individually under the destination card's rules; an ordinary card receives only one.
  - Enchantment-screen previews use the same rules so a Legacy card's existing enchantments remain visible.
- Display permanent entries in the original `[purple]`. Temporary on-play effects use separate `[aqua]` lines; temporary stat enchantments contribute directly to the card's displayed values without another named entry.
- Separate presentation does not relax the one-enchantment limit for ordinary cards.
- Known limitation: some base-game code directly checks the slot's enchantment type, including rest-site `Clone` and Thieving Hopper's `Imbued`. Such checks do not find entries inside a composite.
