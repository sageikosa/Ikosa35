using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Longstrider : SpellDef, ISpellMode, IDurableCapable
    {
        public override string DisplayName => @"Longstrider";
        public override string Description => @"+10 Enhancement to land speed";
        public override MagicStyle MagicStyle => new Transformation();

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

        #region public override IEnumerable<SpellComponent> DivineComponents { get; }
        public override IEnumerable<SpellComponent> DivineComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new DivineFocusComponent();
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

        public bool AllowsSpellResistance => false;
        public bool IsHarmless => true;
        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurable(deliver, deliver.TargetingProcess.Targets[0]);
        }

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
            // enhancement bonus to speed
            var _speed = new Delta(10, typeof(Deltas.Enhancement));

            // applied to land movement
            foreach (var _move in (target as Creature)?.Movements.AllMovements.OfType<LandMovement>() ?? new LandMovement[] { })
            {
                _move.Deltas.Add(_speed);
            }

            return _speed;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            ((Delta)source.ActiveAdjunctObject)?.DoTerminate();
        }

        public bool IsDismissable(int subMode)
            => true;

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
            => string.Empty;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Hour(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }
        #endregion
    }
}
