using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class FalseLife : SpellDef, ISpellMode, IDurableCapable, IDurableAnchorCapable
    {
        public override string DisplayName => @"False Life";
        public override string Description => @"Gain 1d10 + 1/level Temp HP for 1 hour/level";
        public override MagicStyle MagicStyle => new Necromancy();

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new MaterialComponent();
                yield break;
            }
        }
        #endregion

        public override IEnumerable<ISpellMode> SpellModes => this.ToEnumerable();

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new PersonalAim(@"Self", actor);
            yield return new RollAim(@"TempHP", @"Temp HP", new DieRoller(10));
            yield break;
        }

        public bool AllowsSpellResistance => false;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurable(deliver, deliver.TargetingProcess.Targets[0], 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.CopyActivityTargetsToSpellEffects(apply);
            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion

        #region IDurableMode Members
        public IEnumerable<int> DurableSubModes => 0.ToEnumerable();

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
        }

        public bool IsDismissable(int subMode)
            => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
            => string.Empty;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Hour(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact) { yield break; }
        #endregion

        #region IDurableAnchorMode Members
        public object OnAnchor(IAdjunctTracker source, IAdjunctable target)
        {
            if ((source is MagicPowerEffect _power)
                && (target is Creature _critter))
            {
                // calculate hp value from roll and caster level
                var _hpVal = Math.Min(_power.CasterLevel, 10) +
                    ((_power.AllTargets.FirstOrDefault(_t => _t.Key.Equals(@"TempHP")) as ValueTarget<int>)?.Value ?? 1);

                // create the TempHP, but keep it disabled...
                var _tempPt = new Delta(_hpVal, typeof(FalseLife));

                // add it as a chunk
                var _chunk = new TempHPChunk(_critter.TempHealthPoints, _tempPt);
                _critter.TempHealthPoints.Add(_chunk);
                return _chunk;
            }
            return null;
        }

        public void OnEndAnchor(IAdjunctTracker source, IAdjunctable target)
        {
            if (target is Creature _critter)
            {
                // remove the chunk
                if (source.AnchoredAdjunctObject is TempHPChunk _chunk)
                {
                    _critter.TempHealthPoints.Remove(_chunk);
                }
            }
        }
        #endregion
    }
}
