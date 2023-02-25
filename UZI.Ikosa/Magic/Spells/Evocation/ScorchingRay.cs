using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Feats;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class ScorchingRay : SpellDef, ISpellMode, IDamageCapable, IPowerDeliverVisualize
    {
        public override string DisplayName => @"Scorching Ray";
        public override string Description => @"1 or more firery rays deal 4d6 fire damage each.";
        public override MagicStyle MagicStyle => new Evocation();

        #region public override IEnumerable<Descriptor> Descriptors { get; }
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new Fire();
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
                yield return this;
                yield break;
            }
        }
        #endregion

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            // max = 1 ray + 1 ray/4 levels above 3 (maxRange=3)
            var _max = new LinearRange(1, 1, 4, 3, 3);

            // ranged touch (near); ray critical; any creature or object
            yield return new TouchAim(@"Target", @"Target", Lethality.AlwaysLethal,
                ImprovedCriticalRayFeat.CriticalThreatStart(actor as Creature),
                this, new FixedRange(1), _max, new NearRange(), 
                new TargetType[] { new ObjectTargetType(), new CreatureTargetType() });
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            // get "Creature" keyed targets of which the creature is truly aware
            var _critter = deliver.Actor as Creature;
            var _targets = (from _t in deliver.TargetingProcess.Targets
                            where _t.Key.Equals(@"Target", StringComparison.OrdinalIgnoreCase)
                            select _t).ToList();

            SpellDef.DeliverDamageToTouchInCluster(deliver, _targets, 30, 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDamage(apply, apply, 0);
        }
        #endregion

        #region IDamageMode Members
        #region public IEnumerable<int> SubModes { get; }
        public IEnumerable<int> DamageSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }
        #endregion

        public IEnumerable<DamageRule> GetDamageRules(int subMode, bool isCriticalHit)
        {
            yield return new EnergyDamageRule(@"Fire.Damage", 
                new DiceRange(@"Fire", DisplayName, new DiceRoller(4, 6)), @"Scorching Ray", EnergyType.Fire);
            yield break;
        }

        public string DamageSaveKey(Interaction workSet, int subMode)
            => string.Empty;

        public bool CriticalFailDamagesItems(int subMode) => false;
        #endregion

        #region IPowerDeliverVisualize Members

        public VisualizeTransferType GetTransferType() => VisualizeTransferType.Beam;
        public VisualizeTransferSize GetTransferSize() => VisualizeTransferSize.Medium;
        public string GetTransferMaterialKey() => @"#C0FF2000";
        public VisualizeSplashType GetSplashType() => VisualizeSplashType.Pulse;
        public string GetSplashMaterialKey() => @"#C0FF0000|#80FF1000|#C0FF0000";

        #endregion
    }
}
