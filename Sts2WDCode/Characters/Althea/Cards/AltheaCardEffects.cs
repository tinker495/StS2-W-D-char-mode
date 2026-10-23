using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

internal static class AltheaCardEffects
{
    internal static async Task Play(AltheaCard card, PlayerChoiceContext context, CardPlay play)
    {
        var owner = card.Owner;
        var self = owner.Creature;
        var combat = self.CombatState!;
        var piles = owner.PlayerCombatState!;
        int amount = card.DynamicVars["Amount"].IntValue;
        decimal block = card.DynamicVars.Block.BaseValue;
        decimal damage = card.DynamicVars.Damage.BaseValue;
        int count = card.DynamicVars.Cards.IntValue;
        CardModel? selected;
        async Task Block(decimal value) => await CreatureCmd.GainBlock(self, value, ValueProp.Move, play);
        async Task SelfPower<T>(decimal value) where T : PowerModel =>
            await PowerCmd.Apply<T>(context, self, value, self, card);
        async Task Attack(bool all = false, bool random = false, int hits = 1, decimal? value = null, bool copied = false)
        {
            if (combat.HittableEnemies.Count() == 0) return;
            decimal attackDamage = value ?? damage;
            if (copied && self.GetPower<AltheaNextDoublePower>() is { } nextDouble)
                attackDamage *= nextDouble.ModifyDamageMultiplicative(null, attackDamage, ValueProp.Move, self, card);
            var attack = DamageCmd.Attack(attackDamage).FromCard(card).WithHitCount(hits);
            if (copied) attack.Unpowered(); // Already includes the source card's attacker-side preview modifiers.
            if (all) attack.TargetingAllOpponents(combat);
            else if (random) attack.TargetingRandomOpponents(combat);
            else if (play.Target?.IsAlive == true) attack.Targeting(play.Target);
            else return;
            await attack.Execute(context);
            if (card.Kind == AltheaCardKind.UltimateStrike && attack.Results.SelectMany(r => r).Any(r => r.WasTargetKilled))
            {
                var permanent = owner.RunState.CloneCard(card);
                await CardPileCmd.Add(permanent, PileType.Deck);
            }
            if (copied && self.GetPower<VigorPower>() is { } vigor) await PowerCmd.Remove(vigor);
        }
        async Task<CardModel?> SelectAttack() => (await CardSelectCmd.FromHand(context, owner,
            new CardSelectorPrefs(card.ChoicePrompt, 1), c => c.Type == CardType.Attack, card)).FirstOrDefault();
        async Task<List<CardModel>> ChooseAny() => (await CardSelectCmd.FromHand(context, owner,
            new CardSelectorPrefs(card.ChoicePrompt, 0, 999999), null, card)).ToList();
        async Task Wraiths(int number, PileType pile)
        {
            for (int i = 0; i < number; i++) await CardPileCmd.AddGeneratedCardToCombat(
                combat.CreateCard<AltheaWraith>(owner), pile, owner,
                pile == PileType.Draw ? CardPilePosition.Random : CardPilePosition.Bottom);
        }
        switch (card.Kind)
        {
            case AltheaCardKind.RageStrike: case AltheaCardKind.FrenziedStrike:
            case AltheaCardKind.DeathStrike: case AltheaCardKind.DoggedStrike:
            case AltheaCardKind.HeavyStrike: case AltheaCardKind.Strike: case AltheaCardKind.UltimateStrike:
            case AltheaCardKind.CursedBladeFate:
                await Attack(); break;
            case AltheaCardKind.Siarcun:
                await Attack(); await CardPileCmd.Draw(context, count, owner); break;
            case AltheaCardKind.Lleurh:
                await Block(block); await CardPileCmd.Draw(context, count, owner); break;
            case AltheaCardKind.Morfiella:
                await SelfPower<StrengthPower>(amount); await SelfPower<DexterityPower>(amount); break;
            case AltheaCardKind.PureWill: await PlayerCmd.GainEnergy(amount, owner); break;
            case AltheaCardKind.Wraith: break;
            case AltheaCardKind.RecklessStrike:
                if (play.Target != null) await PowerCmd.Apply<VulnerablePower>(context, play.Target, amount, self, card);
                await Attack(); await SelfPower<VulnerablePower>(amount); break;
            case AltheaCardKind.SurgingStrike:
                await Attack(); await CardPileCmd.Draw(context, AltheaRules.For(owner).Attacks, owner); break;
            case AltheaCardKind.AnnihilatingStrike: await Attack(all: true, hits: 2); break;
            case AltheaCardKind.HauntedStrike: await Attack(all: true); await Wraiths(1, PileType.Hand); break;
            case AltheaCardKind.DesperateStrike: await Attack(); await SelfPower<WeakPower>(amount); break;
            case AltheaCardKind.WarriorStrike:
                await Attack(); await AltheaRules.Reform(context, card, play.ResultPile); break;
            case AltheaCardKind.KarmicMadness: case AltheaCardKind.Defend: await Block(block); break;
            case AltheaCardKind.Deflect:
                selected = await SelectAttack();
                if (selected != null) { decimal snapshot = AltheaRules.DisplayDamage(selected); await CardCmd.Discard(context, selected); await Block(snapshot); }
                break;
            case AltheaCardKind.AncestralGrudge: await Block(block); await Wraiths(1, PileType.Discard); break;
            case AltheaCardKind.CurseOfNoRetreat: await Block(block); await Wraiths(2, PileType.Draw); break;
            case AltheaCardKind.SoulAdvice:
                foreach (var other in piles.Hand.Cards.Where(c => c != card).TakeRandom(amount, owner.RunState.Rng.CombatCardSelection).ToList())
                    await CardCmd.Exhaust(context, other);
                await CardPileCmd.Draw(context, count, owner); break;
            case AltheaCardKind.SelfDiscipline:
                await Block(piles.ExhaustPile.Cards.Count + (card.IsUpgraded ? piles.DiscardPile.Cards.Count : 0)); break;
            case AltheaCardKind.RisingMadness:
                var drawn = (await CardPileCmd.Draw(context, count, owner)).ToList();
                foreach (var other in drawn.Where(c => c.Type != CardType.Attack && c.Pile?.Type == PileType.Hand)) await CardCmd.Exhaust(context, other);
                break;
            case AltheaCardKind.Undefeated:
                for (int i = 0; i < card.DynamicVars.Repeat.IntValue; i++) await Block(block);
                await SelfPower<FrailPower>(amount); break;
            case AltheaCardKind.BattleInstinct: await SelfPower<AltheaBattleInstinctPower>(1); break;
            case AltheaCardKind.Review:
                selected = (await CardSelectCmd.FromCombatPile(context, piles.DiscardPile, owner, new CardSelectorPrefs(card.ChoicePrompt, 1))).FirstOrDefault();
                if (selected != null) await CardPileCmd.Add(selected, PileType.Hand); break;
            case AltheaCardKind.SharksTooth: await Attack(all: true); await SelfPower<FreeAttackPower>(1); break;
            case AltheaCardKind.LionsLeap: await Attack(); await SelfPower<AltheaNextDoublePower>(1); break;
            case AltheaCardKind.OrcasAmbush: await Attack(); await SelfPower<AltheaNextDrainPower>(1); break;
            case AltheaCardKind.Disarm: await SelfPower<AltheaDisarmPower>(amount); await Attack(); break;
            case AltheaCardKind.PiercingHarpoon:
                if (play.Target != null) await CreatureCmd.LoseBlock(play.Target, play.Target.Block);
                await Attack(); break;
            case AltheaCardKind.Slaughter: await Attack(all: true); await SelfPower<VulnerablePower>(1); break;
            case AltheaCardKind.BloodSea: await Attack(all: AltheaRules.HarmfulCount(owner) > 0); break;
            case AltheaCardKind.RevenantsRevenge:
                await Attack(random: true, hits: 1 + piles.ExhaustPile.Cards.Count(AltheaRules.IsCurse)); break;
            case AltheaCardKind.ScabbardSwing:
                selected = await SelectAttack();
                if (selected != null) { decimal snapshot = AltheaRules.DisplayDamage(selected); await CardCmd.Discard(context, selected); await Attack(all: true, value: snapshot, copied: true); }
                break;
            case AltheaCardKind.Feint: await Attack(random: true); await CardPileCmd.Draw(context, count, owner); break;
            case AltheaCardKind.SoulShield:
                int exhausted = 0;
                foreach (var other in piles.Hand.Cards.Where(c => c != card).ToList())
                { await CardCmd.Exhaust(context, other); if (other.Pile?.Type == PileType.Exhaust) exhausted++; }
                await Block(block * exhausted); break;
            case AltheaCardKind.Breakthrough:
                if (!card.ClashAtPlay) break;
                selected = (await CardSelectCmd.FromCombatPile(context, piles.DrawPile, owner, new CardSelectorPrefs(card.ChoicePrompt, 1))).FirstOrDefault();
                if (selected != null) await CardPileCmd.Add(selected, PileType.Hand); break;
            case AltheaCardKind.FleetFoot:
                await SelfPower<AltheaFleetFootPower>(count); await CardPileCmd.Draw(context, count, owner); break;
            case AltheaCardKind.InstinctiveDefense:
                int repeats = self.HasPower<FrailPower>() ? 2 : 1;
                for (int i = 0; i < repeats; i++) await Block(block); break;
            case AltheaCardKind.UndyingRage: await Block(block); break;
            case AltheaCardKind.RationalWill:
                foreach (var other in await ChooseAny()) await AltheaRules.Reform(context, other); break;
            case AltheaCardKind.Bloodlust: await SelfPower<VulnerablePower>(1); await PlayerCmd.GainEnergy(3, owner); break;
            case AltheaCardKind.WhisperingWraith:
                await Wraiths(2, PileType.Hand); await Block(block * piles.Hand.Cards.Count(AltheaRules.IsCurse)); break;
            case AltheaCardKind.RestlessMadness: await SelfPower<AltheaRestlessMadnessPower>(amount); break;
            case AltheaCardKind.SharkStance: await SelfPower<AltheaSharkStancePower>(amount); break;
            case AltheaCardKind.LionStance: await SelfPower<AltheaLionStancePower>(amount); break;
            case AltheaCardKind.OrcaStance: await SelfPower<AltheaOrcaStancePower>(amount); break;
            case AltheaCardKind.SoulHeat:
                await SelfPower<AltheaSoulHeatPower>(amount);
                await SelfPower<StrengthPower>(amount * AltheaRules.For(owner).CursesExhausted); break;
            case AltheaCardKind.CurseHarvest: await SelfPower<AltheaCurseHarvestPower>(amount); break;
            case AltheaCardKind.ImmortalObsession: await SelfPower<AltheaImmortalObsessionPower>(block); break;
            case AltheaCardKind.FightingSpirit: await SelfPower<AltheaFightingSpiritPower>(amount); break;
            case AltheaCardKind.PathToGreatness: await SelfPower<AltheaPathToGreatnessPower>(amount); break;
            case AltheaCardKind.FirstWave: await Attack(random: true, hits: 3); break;
            case AltheaCardKind.Rawenaid:
                var attacks = piles.Hand.Cards.Where(c => c != card && c.Type == CardType.Attack).ToList();
                decimal total = attacks.Sum(AltheaRules.DisplayDamage);
                await CardCmd.Discard(context, attacks); await Attack(all: true, value: total, copied: true); break;
            case AltheaCardKind.GenMorfiClad: await Attack(random: true, hits: 1 + AltheaRules.HarmfulCount(owner)); break;
            case AltheaCardKind.CrushingStrike:
                await Attack();
                if (play.Target?.IsAlive == true) await PowerCmd.Apply<WeakPower>(context, play.Target, amount, self, card);
                await SelfPower<FrailPower>(amount); break;
            case AltheaCardKind.Struggle:
                await Attack(all: true);
                var reserved = piles.DrawPile.Cards.Where(c => c.Type == CardType.Attack)
                    .TakeRandom(amount, owner.RunState.Rng.CombatCardSelection).ToList();
                foreach (var other in reserved) await CardPileCmd.Add(other, PileType.Play);
                foreach (var other in reserved)
                {
                    if (!self.IsAlive || other.Pile?.Type != PileType.Play) continue;
                    other.ExhaustOnNextPlay = true;
                    await CardCmd.AutoPlay(context, other, null);
                }
                break;
            case AltheaCardKind.Regroup: await CardPileCmd.Draw(context, count, owner); break;
            case AltheaCardKind.Overcome: await SelfPower<AltheaOvercomePower>(block); break;
            case AltheaCardKind.BloodWhisper:
                await Attack();
                while (piles.Hand.Cards.Count < CardPile.MaxCardsInHand)
                {
                    var curse = piles.DrawPile.Cards.FirstOrDefault(AltheaRules.IsCurse);
                    if (curse == null) break;
                    await CardPileCmd.Add(curse, PileType.Draw, CardPilePosition.Top);
                    if (!(await CardPileCmd.Draw(context, 1, owner)).Any()) break;
                }
                break;
            case AltheaCardKind.DemonicBlade:
                foreach (var target in combat.HittableEnemies.ToList())
                {
                    await PowerCmd.Apply<WeakPower>(context, target, amount, self, card);
                    await PowerCmd.Apply<VulnerablePower>(context, target, amount, self, card);
                }
                await SelfPower<WeakPower>(amount); await SelfPower<VulnerablePower>(amount); break;
            case AltheaCardKind.Liberation:
                foreach (var other in piles.DiscardPile.Cards.ToList()) await AltheaRules.Reform(context, other); break;
            case AltheaCardKind.GatheringSouls:
                await Wraiths(amount, PileType.Hand);
                await PlayerCmd.GainEnergy(piles.Hand.Cards.Count(AltheaRules.IsCurse), owner); break;
            case AltheaCardKind.VesselOfDelusion:
                int x = card.ResolveEnergyXValue();
                for (int i = 0; i < x; i++) await Block(block);
                await Wraiths(x, PileType.Draw); break;
            case AltheaCardKind.ResoluteWill:
                selected = (await CardSelectCmd.FromHand(context, owner,
                    new CardSelectorPrefs(card.ChoicePrompt, 1), c => c.IsTransformable, card)).FirstOrDefault();
                if (selected != null) await AltheaRules.Reform(context, selected);
                await SelfPower<AltheaReformEchoPower>(1); break;
            case AltheaCardKind.WarriorsCreed:
                await SelfPower<StrengthPower>(amount); await SelfPower<AltheaWarriorsCreedPower>(amount);
                await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Random); break;
            case AltheaCardKind.InnerReserve: await SelfPower<AltheaInnerReservePower>(amount); break;
            case AltheaCardKind.UnrivaledBlade: await SelfPower<AltheaUnrivaledBladePower>(amount); break;
            case AltheaCardKind.SoulLegion: await SelfPower<AltheaSoulLegionPower>(amount); break;
            case AltheaCardKind.ArchipelagoArts: await SelfPower<AltheaArchipelagoArtsPower>(amount); break;
            case AltheaCardKind.SeaWarrior: await SelfPower<AltheaSeaWarriorPower>(amount); break;
            case AltheaCardKind.ErosionCurse: await SelfPower<AltheaErosionCursePower>(amount); break;
            case AltheaCardKind.SoulVeil:
                foreach (var player in combat.Players.Where(p => p.Creature.IsAlive && p.PlayerCombatState != null))
                    for (int i = 0; i < amount; i++)
                        await CardPileCmd.AddGeneratedCardToCombat(combat.CreateCard<AltheaWraith>(player), PileType.Hand, owner);
                break;
            case AltheaCardKind.SeaParting: await Attack(all: true); break;
            case AltheaCardKind.CurseDominion:
                var curses = combat.Players.Where(p => p != owner && p.PlayerCombatState != null)
                    .SelectMany(p => new[] { p.PlayerCombatState!.Hand, p.PlayerCombatState.DrawPile, p.PlayerCombatState.DiscardPile })
                    .SelectMany(pile => pile.Cards).Where(AltheaRules.IsCurse).ToList();
                foreach (var curse in curses) await AltheaRules.TakeCombatCard(curse, owner);
                break;
            case AltheaCardKind.FinalBlow: await Attack(); break;
            case AltheaCardKind.FightersForm: await SelfPower<AltheaFightersFormPower>(amount); break;
            default: throw new InvalidOperationException($"Missing Althea card effect: {card.Kind}");
        }
    }
}
