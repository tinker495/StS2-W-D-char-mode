# Masterwork Repeats Effects Within One Card Play

Masterwork (역작) is an X-cost theme that repeats a card's effect and enchantment effects X times, where X is energy spent. It does not count as multiple card plays through the base game's replay mechanism. This avoids multiplying effects that count card plays while rewarding accumulated enchantments by rerunning on-play enchantment effects such as Adroit on each repetition.

## Considered Options

- **Use existing replay (`BaseReplayCount`).** Simple to implement, but multiplies both card-play counts and on-play effects by X, contrary to the design. Rejected.

## Consequences

- Vigor applies to all repeated attacks and is consumed once after the complete execution. Base damage 4, X=3, and Vigor 2 produce three hits of 6 damage. Because `VigorPower` is consumed after one `AttackCommand`, structure repeated attacks so it is not consumed between repetitions. ([ADR-0001](0001-reuse-base-game-effects.md))
- X=0 produces no repetitions, damage, or enchantment effects.
- Stop remaining repetitions if the target dies.
- Check detailed interactions with enchantments other than Adroit when adding the relevant cards.
