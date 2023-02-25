using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Alchemal
{
    [
    Serializable,
    ItemInfo(@"Glow row", @"Alchemal item that sheds light", @"glow_rod")
    ]
    public class GlowRod : ItemBase, IActionProvider, IDoActivity
    {
        public GlowRod()
            : base(@"Glow Rod", Size.Miniature)
        {
            Price.CorePrice = 2;
            BaseWeight = 1;
            ItemMaterial = IronMaterial.Static;
            MaxStructurePoints.BaseValue = IronMaterial.Static.StructurePerInch * 1;
        }

        private bool _Activated = false;

        public bool Activated => _Activated;

        #region IActionProvider Members

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (!Activated)
            {
                var _regular = new ActionTime(TimeType.Regular);
                yield return new DoActivity(this, this, _regular, _regular, false,
                    @"Light", @"Starts shedding light, which lasts for 6 hours", @"101");
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => GetInfoData.GetInfoFeedback(this, budget.Actor);

        #endregion

        #region IDoActivity Members

        public CoreStep DoPerformActivity(CoreActivity activity)
        {
            if ((activity.Action is DoActivity _doActivity) && _doActivity.Key.Equals(@"Light", StringComparison.OrdinalIgnoreCase))
            {
                if (_doActivity.Budget is LocalActionBudget _budget)
                {
                    var _illuminate = new Illumination(this, 30, 60, false);
                    var _expiry = new Expiry(_illuminate, _budget.TurnTick.TurnTracker.Map.CurrentTime + Hour.UnitFactor * 6, TimeValTransition.Entering, Round.UnitFactor);
                    AddAdjunct(_expiry);
                    _Activated = true;
                    return activity.GetActivityResultNotifyStep(@"Activated"); // Name?
                }

                return activity.GetActivityResultNotifyStep(@"No map context to get time");
            }
            return activity.GetActivityResultNotifyStep(@"Activity missing or invalid action key");
        }

        public ObservedActivityInfo DoGetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            // NOTE: only one action provided by this IDoActivity instance
            return ObservedActivityInfoFactory.CreateInfo(@"Light", activity.Actor, observer, this);
        }

        public IEnumerable<AimingMode> DoAimingMode(CoreActivity activity)
        {
            yield break;
        }

        #endregion

        protected override string ClassIconKey => string.Empty;
    }
}
