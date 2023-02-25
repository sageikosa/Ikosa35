using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class StartRage : ActionBase
    {
        public StartRage(IActionSource actionSource, int physical, int save, int recoveryRounds, string orderKey, params Adjunct[] extras)
            : base(actionSource, new ActionTime(Contracts.TimeType.Free), false, false, orderKey)
        {
            _Physical = physical;
            _Saves = save;
            _Recovery = recoveryRounds;
            _Extras = extras;
        }

        #region data
        private int _Physical;
        private int _Saves;
        private int _Recovery;
        private Adjunct[] _Extras;
        #endregion

        public int PhysicalBoost => _Physical;
        public int SaveBoost => _Saves;
        public int RecoveryRounds => _Recovery;
        public IEnumerable<Adjunct> Extras => _Extras.Select(_e => _e);

        public override string Key => @"Rage.Start";
        public override string DisplayName(CoreActor actor) => @"Start Raging";
        public override bool IsMental => true;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Looking angry", activity.Actor, observer);

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity?.Actor?.AddAdjunct(new Raging(ActionSource, PhysicalBoost, SaveBoost, RecoveryRounds, false, _Extras));
            return new RegisterActivityStep(activity, Budget);
        }
    }
}
