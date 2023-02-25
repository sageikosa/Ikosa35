using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class DisruptUndead : SpellDef, ISpellMode, IDamageCapable, IPowerDeliverVisualize
    {
        public override string DisplayName => @"Disrupt Undead";
        public override string Description => @"Ranged touch deals 1d6 to one undead";
        public override MagicStyle MagicStyle => new Necromancy();

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
            yield return new TouchAim(@"Undead Creature", @"Undead Creature", Lethality.AlwaysLethal,
                ImprovedCriticalRangedTouchFeat.CriticalThreatStart(actor as Creature),
                this, new FixedRange(1), new FixedRange(1), new NearRange(), new CreatureTargetType());
            yield break;
        }
        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDamageToTouch(deliver, 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            if ((apply.DeliveryInteraction.Target is Creature _critter)
                && (_critter.CreatureType is UndeadType))
            {
                // only undead can be damaged by this spell
                SpellDef.ApplyDamage(apply, apply, 0);
            }
        }
        #endregion

        #region IDamageSpellMode Members
        public IEnumerable<int> DamageSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }

        public IEnumerable<DamageRule> GetDamageRules(int subMode, bool isCriticalHit)
        {
            yield return new EnergyDamageRule(@"Positive.Damage", new DiceRange(@"Disruption", DisplayName, new DieRoller(6)), @"Disruption", EnergyType.Positive);
            if (isCriticalHit)
                yield return new EnergyDamageRule(@"Positive.Damage.Critical", new DiceRange(@"Disruption (Critical)", DisplayName, new DieRoller(6)), @"Disruption (Critical)", EnergyType.Positive);
            yield break;
        }

        public string DamageSaveKey(Interaction workSet, int subMode)
            => string.Empty;

        public bool CriticalFailDamagesItems(int subMode) => false;
        #endregion

        #region IPowerDeliverVisualize Members

        public VisualizeTransferType GetTransferType() { return VisualizeTransferType.Beam; }
        public VisualizeTransferSize GetTransferSize() { return VisualizeTransferSize.Small; }
        public string GetTransferMaterialKey() { return @"#E0FFFFFF"; }
        public VisualizeSplashType GetSplashType() { return VisualizeSplashType.Pulse; }
        public string GetSplashMaterialKey() { return @"#C0FFFFFF|#80FFFFFF|#C0FFFFFF"; }

        #endregion
    }
}
