using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class TrueStrike : SpellDef, ISpellMode, IDurableCapable
    {
        public override string DisplayName { get { return @"True Strike"; } }
        public override string Description { get { return @"+20 insight delta on next attack.  Never miss on a miss chance check."; } }
        public override MagicStyle MagicStyle { get { return new Divination(Divination.SubDivination.Precognition); } }

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new FocusComponent();
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
            yield return new PersonalAim(@"Self", actor);
            yield break;
        }

        public bool AllowsSpellResistance { get { return false; } }
        public bool IsHarmless { get { return true; } }
        public void ActivateSpell(PowerActivationStep<SpellSource> deliver) { SpellDef.DeliverDurable(deliver, deliver.TargetingProcess.Targets[0]); }

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
            TrueStrikeAdjunct _strike = new TrueStrikeAdjunct(source);
            target.AddAdjunct(_strike);
            return _strike;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            TrueStrikeAdjunct _strike = source.ActiveAdjunctObject as TrueStrikeAdjunct;
            if (_strike != null)
            {
                target.RemoveAdjunct(_strike);
            }
        }

        public bool IsDismissable(int subMode) { return false; }
        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode) { return string.Empty; }

        public DurationRule DurationRule(int subMode)
        {
            // ensure duration lasts until the end of the following round
            DurationRule _dr = new DurationRule(DurationType.Span, new SpanRulePart(1, new Round()));
            _dr.RoundUp = true;
            return _dr;
        }

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }
        #endregion
    }

    [Serializable]
    public class TrueStrikeAdjunct : Adjunct, IProcessFeedback
    {
        public TrueStrikeAdjunct(object source)
            : base(source)
        {
            _Delta = new Delta(20, typeof(Deltas.Insight));
        }

        private Delta _Delta;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (Anchor is Creature _critter)
            {
                _critter.MeleeDeltable.Deltas.Add(_Delta);
                _critter.RangedDeltable.Deltas.Add(_Delta);
                _critter.AddIInteractHandler(this);
            }
        }

        protected override void OnDeactivate(object source)
        {
            _Delta.DoTerminate();
            if (Anchor is Creature _critter)
            {
                _critter.RemoveIInteractHandler(this);
            }
            base.OnDeactivate(source);
        }

        #region IProcessFeedback Members
        public void ProcessFeedback(Interaction workSet)
        {
            // deactivate after an attack is made...
            _Delta.Enabled = false;
        }
        #endregion

        #region IInteractHandler Members
        public void HandleInteraction(Interaction workSet)
        {
            if (_Delta.Enabled)
            {
                // add miss chance alteration, to prevent missing
                var _miss = new MissChanceAlteration(workSet.InteractData, this, 100);
                workSet.InteractData.Alterations.Add(workSet.Target, _miss);
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(MeleeAttackData);
            yield return typeof(ReachAttackData);
            yield return typeof(RangedAttackData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // put at front of chain (so we process feedback at the very end)
            return true;
        }
        #endregion

        public override object Clone()
        {
            return new TrueStrikeAdjunct(Source);
        }
    }
}
