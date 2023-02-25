using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class InflictMinorWounds : SpellDef, ISpellMode, IDamageCapable, ISaveCapable
    {
        public override string DisplayName => @"Inflict Minor Wounds";
        public override string Description => @"Inflicts 1 point of damage";
        public override MagicStyle MagicStyle => new Necromancy();
        public override bool ArcaneCharisma => true;

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
            yield return new TouchAim(@"Creature", @"Creature", Lethality.AlwaysLethal,
                ImprovedCriticalTouchFeat.CriticalThreatStart(actor as Creature),
                this, new FixedRange(1), new FixedRange(1), new MeleeRange(), new CreatureTargetType());
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
            if (apply.DeliveryInteraction.Target is Creature _critter)
            {
                if (_critter.CreatureType is UndeadType)
                {
                    // collect all "damage rolls" (additional may have been added by feats, powers etc.)
                    var _total = apply.AllPrerequisites<DamageRollPrerequisite>()
                        .Where(_r => !_r.BindKey.EndsWith(@".Critical"))            // no critical healing of undead
                        .Sum(_r => _r.RollValue);

                    // recover interaction
                    var _recover = new RecoverHealthPointData(apply.Actor, new Deltable(_total), false, true);
                    var _cure = new Interaction(apply.Actor, apply.PowerUse.PowerActionSource, apply.DeliveryInteraction.Target, _recover);
                    _critter.HandleInteraction(_cure);

                    // if there are new unyielded prerequisites, create a rety interaction step
                    if (_cure.Feedback.OfType<PrerequisiteFeedback>().Any(_f => !_f.Yielded))
                    {
                        new RetryInteractionStep(apply, DisplayName, _cure);
                    }
                }
                else if (!_critter.CreatureType.IsLiving)
                {
                    // cannot be inflicted
                }
                else
                {
                    SpellDef.ApplyDamage(apply, apply, 0);
                }
            }
        }
        #endregion

        #region IDamageMode Members
        public IEnumerable<int> DamageSubModes { get { yield return 0; } }

        public virtual IEnumerable<DamageRule> GetDamageRules(int subMode, bool isCriticalHit)
        {
            yield return new EnergyDamageRule(@"Inflict.Negative", FixedRange.One, @"Inflict Minor", EnergyType.Negative);
            if (isCriticalHit)
                yield return new EnergyDamageRule(@"Inflict.Negative.Critical", FixedRange.One, @"Inflict Minor (Critical)", EnergyType.Negative);
            yield break;
        }

        public string DamageSaveKey(Interaction workSet, int subMode)
        {
            return @"Save.Will";
        }

        public bool CriticalFailDamagesItems(int subMode) => false;
        #endregion

        #region ISpellSaveMode Members
        public virtual SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
        {
            var _critter = workSet.Target as Creature;
            if (!(_critter.CreatureType is UndeadType))
            {
                return new SaveMode(SaveType.Will, SaveEffect.Negates, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
            }
            return null;
        }
        #endregion
    }
}
