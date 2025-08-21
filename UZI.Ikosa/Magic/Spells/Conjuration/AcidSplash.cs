using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class AcidSplash : SpellDef, ISpellMode, IDamageCapable, IPowerDeliverVisualize
    {
        public override string DisplayName => @"Acid Splash";
        public override string Description => @"Ranged touch orb deals 1d3 acid damage";
        public override MagicStyle MagicStyle => new Conjuration(Conjuration.SubConjure.Creation);

        #region public override IEnumerable<Descriptor> Descriptors { get; }
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new Acid();
                yield break;
            }
        }
        #endregion

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield break;
            }
        }
        #endregion

        #region public override IEnumerable<ISpellMode> SpellModes { get; }
        public override IEnumerable<ISpellMode> SpellModes
        {
            get
            {
                yield return (ISpellMode)this;
                yield break;
            }
        }
        #endregion

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Target", @"Target", Lethality.AlwaysLethal,
                ImprovedCriticalRangedTouchFeat.CriticalThreatStart(actor),
                this, new FixedRange(1), new FixedRange(1), new NearRange(),
                new TargetType[] { new ObjectTargetType(), new CreatureTargetType() });
            yield break;
        }

        public bool AllowsSpellResistance => false;
        public bool IsHarmless => false;
        #endregion

        #region public void Deliver(PowerDeliveryStep<SpellSource> deliver)
        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDamageToTouch(deliver, 0);
        }
        #endregion

        #region public void ApplySpell(PowerApplyStep<SpellSource> apply)
        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDamage(apply, apply, 0);
        }
        #endregion

        #region IDamageSpellMode Members
        public IEnumerable<DamageRule> GetDamageRules(int subMode, bool isCriticalHit)
        {
            yield return new EnergyDamageRule(@"Acid.Damage", new DiceRange(@"Acid", DisplayName, new DieRoller(3)), @"Acid Damage", EnergyType.Acid);
            if (isCriticalHit)
            {
                yield return new EnergyDamageRule(@"Acid.Damage.Critical", new DiceRange(@"Acid (Critical)", DisplayName, new DieRoller(3)), @"Acid Damage (Critical)", EnergyType.Acid);
            }

            yield break;
        }

        public IEnumerable<int> DamageSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }

        public string DamageSaveKey(Interaction workSet, int subMode)
        {
            return string.Empty;
        }

        public bool CriticalFailDamagesItems(int subMode) => false;
        #endregion

        #region IPowerDeliverVisualize Members

        public VisualizeTransferType GetTransferType() { return VisualizeTransferType.Orb; }
        public VisualizeTransferSize GetTransferSize() { return VisualizeTransferSize.Small; }
        public string GetTransferMaterialKey() { return @"#E020FF00"; }
        public VisualizeSplashType GetSplashType() { return VisualizeSplashType.Pop; }
        public string GetSplashMaterialKey() { return @"#C020FF00|#FF60FF00|#C020FF00"; }

        #endregion
    }
}
