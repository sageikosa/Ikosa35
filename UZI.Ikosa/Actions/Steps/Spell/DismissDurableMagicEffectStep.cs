using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class DismissDurableMagicEffectStep : CoreStep
    {
        public DismissDurableMagicEffectStep(CoreActivity activity, DurableMagicEffect effect)
            : base(activity)
        {
            _Effect = effect;
        }

        #region state
        private DurableMagicEffect _Effect;
        #endregion

        public CoreActivity Activity => Process as CoreActivity;
        public DurableMagicEffect DurableMagicEffect => _Effect;

        public override bool IsDispensingPrerequisites => false;
        protected override StepPrerequisite OnNextPrerequisite() => null;

        protected override bool OnDoStep()
        {
            // get display before eject
            var _display = Activity.Action.DisplayName(Activity.Actor);
            _Effect.Eject();

            var _step = Activity.GetActivityResultNotifyStep(_display);
            _step.AppendFollowing(Activity.GetNotifyStep(
                new RefreshNotify(true, true, true, true, true)));
            return true;
        }
    }
}
