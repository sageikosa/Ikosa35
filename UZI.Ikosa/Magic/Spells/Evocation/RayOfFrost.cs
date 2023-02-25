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
    public class RayOfFrost : SpellDef, ISpellMode, IDamageCapable, IPowerDeliverVisualize
    {
        public override string DisplayName => @"Ray of Frost";
        public override string Description => @"Ranged touch attack for 1d3 cold damage";
        public override MagicStyle MagicStyle => new Evocation();

        #region public override IEnumerable<Descriptor> Descriptors { get; }
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new Cold();
                yield break;
            }
        }
        #endregion

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, false, false, false);

        public override IEnumerable<ISpellMode> SpellModes
            => this.ToEnumerable();

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Target", @"Target", Lethality.AlwaysLethal,
                ImprovedCriticalRayFeat.CriticalThreatStart(actor as Creature),
                this, new FixedRange(1), new FixedRange(1), new NearRange(), new TargetType[] { new ObjectTargetType(), new CreatureTargetType() });
            yield break;
        }
        public bool AllowsSpellResistance => true;
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

        #region IDamageCapable Members
        public IEnumerable<DamageRule> GetDamageRules(int subMode, bool isCriticalHit)
        {
            yield return new EnergyDamageRule(@"Cold.Damage", new DiceRange(@"Cold", DisplayName, new DieRoller(3)), @"Cold Damage", EnergyType.Cold);
            if (isCriticalHit)
                yield return new EnergyDamageRule(@"Cold.Damage.Critical", new DiceRange(@"Cold (Critical)", DisplayName, new DieRoller(3)), @"Cold Damage (Critical)", EnergyType.Cold);
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
            => string.Empty;

        public bool CriticalFailDamagesItems(int subMode) => false;
        #endregion

        #region IPowerDeliverVisualize Members

        public VisualizeTransferType GetTransferType() => VisualizeTransferType.FullSurge;
        public VisualizeTransferSize GetTransferSize() => VisualizeTransferSize.Small;
        public string GetTransferMaterialKey() => @"#E000FFFF";
        public VisualizeSplashType GetSplashType() => VisualizeSplashType.Pulse;
        public string GetSplashMaterialKey() => @"#C000FFFF|#8040FFFF|#C000FFFF";

        #endregion
    }
}
