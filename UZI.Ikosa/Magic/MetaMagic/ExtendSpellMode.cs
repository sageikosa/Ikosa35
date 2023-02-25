using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class ExtendSpellMode : MetaMagicSpellMode, IDurableCapable
    {
        public ExtendSpellMode(ISpellMode wrapped)
            : base(wrapped)
        {
        }

        public override IMode GetCapability<IMode>()
        {
            // see if there is an unmodified version of the desired mode
            var _baseMode = base.GetCapability<IMode>();
            if (_baseMode != null)
            {
                // if so, replace if it matches one of our supported modes
                switch (_baseMode)
                {
                    case IDurableCapable _:
                        return this as IMode;
                }
            }

            // pass through (NULL or original)
            return _baseMode;
        }

        #region IDurableMode Members

        public IEnumerable<int> DurableSubModes
            => Wrapped.GetCapability<IDurableCapable>().DurableSubModes;

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
            => Wrapped.GetCapability<IDurableCapable>().Activate(source, target, subMode, activateSource);

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
            => Wrapped.GetCapability<IDurableCapable>().Deactivate(source, target, subMode, deactivateSource);

        public bool IsDismissable(int subMode)
            => Wrapped.GetCapability<IDurableCapable>().IsDismissable(subMode);

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => Wrapped.GetCapability<IDurableCapable>().DurableSaveKey(targets, workSet, subMode);

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Wrapped.GetCapability<IDurableCapable>().GetDurableModePrerequisites(subMode, interact);

        public DurationRule DurationRule(int subMode)
        {
            var _rule = Wrapped.GetCapability<IDurableCapable>().DurationRule(subMode);
            switch (_rule.DurationType)
            {
                case DurationType.Span:
                case DurationType.ConcentrationPlusSpan:
                    // double the numer of units
                    _rule = new DurationRule(_rule.DurationType,
                        _rule.SpanParts.Select(_sp => new SpanRulePart(_sp.NumberUnits * 2, _sp.TimeUnit, _sp.LevelFactor)))
                    {
                        MaxLevel = _rule.MaxLevel
                    };
                    break;
            }
            return _rule;
        }

        #endregion
    }
}
