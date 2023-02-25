using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Wraps most basic processing and information for simple activities</summary>
    /// <remarks>Does not provoke from target(s), and uses budget to reactively register activity</remarks>
    [Serializable]
    public class DoActivity : ActionBase
    {
        /// <summary>Wraps most basic processing and information for simple activities</summary>
        /// <remarks>Does not provoke from target(s), and uses budget to reactively register activity</remarks>
        public DoActivity(IActionSource source, IDoActivity doer, ActionTime needed, ActionTime cost, bool provokesMelee, string key, string displayName, string orderKey)
            : base(source, needed, cost, provokesMelee, false, orderKey)
        {
            _Key = key;
            _DisplayName = displayName;
            _Doer = doer;
        }

        #region private data
        private string _Key;
        private string _DisplayName;
        private IDoActivity _Doer;
        #endregion

        public override string Key => _Key;
        public override string DisplayName(CoreActor actor) => _DisplayName;

        public override bool IsStackBase(Core.CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => _Doer.DoGetActivityInfo(activity, observer);

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity.EnqueueRegisterPreEmptively(Budget);
            return _Doer.DoPerformActivity(activity);
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
            => _Doer.DoAimingMode(activity);

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }

    /// <summary>Implemented by objects with relatively simple action needs (usually only 1 or two simple actions)</summary>
    public interface IDoActivity : IItemBase
    {
        CoreStep DoPerformActivity(CoreActivity activity);
        ObservedActivityInfo DoGetActivityInfo(CoreActivity activity, CoreActor observer);
        IEnumerable<AimingMode> DoAimingMode(CoreActivity activity);
    }
}