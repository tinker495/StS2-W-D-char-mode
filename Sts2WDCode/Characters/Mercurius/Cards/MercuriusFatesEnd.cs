using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusFatesEnd() : MercuriusCard(1, CardType.Attack, CardRarity.Rare, TargetType.RandomEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        while (!CombatManager.Instance.IsOverOrEnding)
        {
            var target = Owner.RunState.Rng.CombatTargets.NextItem(CombatState!.HittableEnemies);
            if (target == null) break;
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(target).Execute(context);
            var drawn = (await CardPileCmd.Draw(context, 1, Owner)).FirstOrDefault();
            if (drawn == null || !drawn.Keywords.Contains(CardKeyword.Ethereal)) break;
        }
    }
    protected override void OnUpgrade() { DynamicVars.Damage.UpgradeValueBy(5m); }
}
