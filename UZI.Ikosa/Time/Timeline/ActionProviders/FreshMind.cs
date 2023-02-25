using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Time
{
    /// <summary>
    /// Applied to creature after appropriate rest
    /// </summary>
    [Serializable]
    public class FreshMind : Adjunct, ITimelineActions, ITimelineInterruptable
    {
        /// <summary>
        /// Applied to creature after appropriate rest
        /// </summary>
        public FreshMind(ICasterClass casterClass)
            : base(casterClass)
        {
            // NOTE: spell slot recharging rest can add this
            // TODO: divine casters get this differently...
            _Interrupted = false;
        }

        #region data
        private bool _Interrupted;
        #endregion

        public ICasterClass CasterClass => Source as ICasterClass;
        public bool Interrupted => _Interrupted;

        public override object Clone()
            => new FreshMind(CasterClass);

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (Interrupted && ((CasterClass as ISlottedCasterBaseClass)?.MustRestToRecharge ?? false))
            {
                yield return new RegainFreshMind(new ActionTime(Hour.UnitFactor, Contracts.TimeType.TimelineScheduling));
            }
            else
            {
                var _critter = Anchor as Creature;
                if (CasterClass is ISpontaneousCaster _spontaneous)
                {
                    yield return new RechargeSlots(_critter, _spontaneous,
                        new ActionTime(Minute.UnitFactor * 15, TimeType.TimelineScheduling));   // recharge for spontaneous
                }
                if (CasterClass is IPreparedCasterClass _prepared)
                {
                    yield return new PrepareSpells(_critter, _prepared);   // recharge and prepare?
                }
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => new TimelineActionProviderInfo($@"Fresh Mind", ID);

        public void EnteringTimeline()
        {
        }

        public void LeavingTimeline()
        {
        }

        public void Interrupt()
        {
            _Interrupted = true;
        }

        public void RegainFreshMind()
        {
            _Interrupted = false;
        }
    }
}
