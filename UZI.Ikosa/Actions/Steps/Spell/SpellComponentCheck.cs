using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class SpellComponentCheck : CoreStep
    {
        public SpellComponentCheck(CoreActivity activity, CoreStep follower)
            : base(activity)
        {
            _Follower = follower;
        }

        #region data
        private CoreStep _Follower;
        #endregion

        public CoreActivity Activity => Process as CoreActivity;
        public override bool IsDispensingPrerequisites => false;

        protected override StepPrerequisite OnNextPrerequisite() => null;

        protected override bool OnDoStep()
        {
            // look at components that have been setup to finalize
            var _components = Activity.Targets
                .OfType<SpellComponentFinalizeTarget>()
                .Select(_c => _c.SpellComponent)
                .ToList();

            if (_components.All(_c => !_c.HasFailed))
            {
                AppendFollowing(_Follower);
            }

            return true;
        }
    }
}
