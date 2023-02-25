using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Services;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa
{
    [Serializable]
    public class DyingEffect : Adjunct, ITrackTime, IActionFilter, IActionProvider
    {
        /*
        unconscious and near death. 
         * no actions and unconscious. 
        At the end of each round (starting with the round in which the character dropped below 0 hit points), 
         *   the character rolls d% to become stable. (Self-stabilizing action)
         *   10% chance to stabilize. otherwise, loses 1 hit point. 
        reaches -10 hit points ==> dead. 
         */

        public DyingEffect(IActionSource source)
            : base(source)
        {
        }

        protected override void OnActivate()
        {
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
        }

        #region ITrackTime Members
        public void TrackTime(double timeVal)
        {
            // TODO:
            throw new NotImplementedException();
        }

        public double Resolution
        {
            get { return Round.UnitFactor; }
        }
        #endregion

        #region IActionFilter Members

        public bool SuppressAction(object source, CoreActionBudget budget, CoreAction action)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IActionProvider Members

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            LocalActionBudget _budget = budget as LocalActionBudget;
            // TODO: budget...

            // TODO: self-stabilize action
            throw new NotImplementedException();
        }

        public Info GetProviderInfo(CoreActionBudget budget)
        {
            return new AdjunctInfo(@"Dying");
        }

        #endregion

        public override object Clone()
        {
            return new DyingEffect(Source as IActionSource);
        }
    }
}