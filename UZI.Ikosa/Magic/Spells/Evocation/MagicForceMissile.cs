using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Core.Dice;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class MagicForceMissile : SpellDef, ISpellMode, IDamageCapable, IPowerDeliverVisualize
    {
        public override string DisplayName => @"Magic Force Missile";
        public override string Description => @"1 or more bolts of magic force energy deal 1d4+1 damage each.";
        public override MagicStyle MagicStyle => new Evocation();

        #region public override IEnumerable<Descriptor> Descriptors { get; }
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new Force();
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
            // max = 1 missile + 1 missile/2 levels above 1 (maxRange=5)
            var _max = new LinearRange(1, 1, 2, 1, 5);
            yield return new AwarenessAim(@"Creature", @"Creature", FixedRange.One, _max, new MediumRange(), new CreatureTargetType()) { AllowDuplicates = true };
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            // get "Creature" keyed targets of which the creature is truly aware
            var _critter = deliver.Actor as Creature;
            var _targets = (from _t in deliver.TargetingProcess.Targets
                            where _t.Key.Equals(@"Creature", StringComparison.OrdinalIgnoreCase)
                            && (_critter.Awarenesses.GetAwarenessLevel(_t.Target.ID) == AwarenessLevel.Aware)
                            select _t).ToList();

            SpellDef.DeliverDamageInCluster(deliver, _targets, false, true, 15, 0);
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
            yield return new EnergyDamageRule(@"Force.Damage", new DiceRange(@"Force", DisplayName, new ComplexDiceRoller(@"1d4+1")), @"Magic Force Missile", EnergyType.Force);
            yield break;
        }

        public string DamageSaveKey(Interaction workSet, int subMode)
            => string.Empty;

        public bool CriticalFailDamagesItems(int subMode) => false;
        #endregion

        #region IPowerDeliverVisualize Members

        public VisualizeTransferType GetTransferType() => VisualizeTransferType.ConeBolt;
        public VisualizeTransferSize GetTransferSize() => VisualizeTransferSize.Small;
        public string GetTransferMaterialKey() => @"#C0FFFF00";
        public VisualizeSplashType GetSplashType() => VisualizeSplashType.Pop;
        public string GetSplashMaterialKey() => @"#C0FFFF00|#80FFFF00|#C0FFFF00";

        #endregion
    }
}