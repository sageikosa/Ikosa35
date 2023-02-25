using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Heroism : SpellDef, ISpellMode, IDurableCapable, ISaveCapable
    {
        public override string DisplayName => @"Heroism";
        public override string Description => @"+2 Moral on attacks, all saves and all skill checks.";
        public override MagicStyle MagicStyle
            => new Enchantment(Enchantment.SubEnchantment.Compulsion);

        #region public override IEnumerable<Descriptor> Descriptors { get; }
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new MindAffecting();
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
            yield return new TouchAim(@"Creature", @"Creature", Lethality.AlwaysNonLethal,
                20, this, FixedRange.One, FixedRange.One, new MeleeRange(), new CreatureTargetType());
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        #region public void Deliver(PowerDeliveryStep<SpellSource> deliver)
        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurableToTouch(deliver, @"Creature", 0);
        }
        #endregion

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion

        #region IDurableMode Members
        public IEnumerable<int> DurableSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (target is Creature _critter)
            {
                var _morale = new Delta(2, typeof(Deltas.Morale));
                // all attacks
                _critter.MeleeDeltable.Deltas.Add(_morale);
                _critter.RangedDeltable.Deltas.Add(_morale);
                _critter.OpposedDeltable.Deltas.Add(_morale);
                // all saves
                _critter.WillSave.Deltas.Add(_morale);
                _critter.ReflexSave.Deltas.Add(_morale);
                _critter.FortitudeSave.Deltas.Add(_morale);
                // all skills
                _critter.ExtraSkillCheck.Deltas.Add(_morale);
                return _morale;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source.ActiveAdjunctObject as Delta)?.DoTerminate();
        }

        public bool IsDismissable(int subMode) => false;
        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
            => @"Save.Will";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(10, new Minute(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact) { yield break; }

        #endregion

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource,Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Will, SaveEffect.Negates, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion
    }
}
