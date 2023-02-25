using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    [Serializable]
    public class ReactorStepCompleteGroup : AdjunctGroup, ICanReactToStepComplete
    {
        public ReactorStepCompleteGroup(object source)
            : base(source)
        {
        }

        public void ReactToStepComplete(CoreStep step)
        {
            foreach (var _reactor in Members.OfType<ICanReactToStepComplete>())
            {
                _reactor.ReactToStepComplete(step);
            }
        }

        public bool CanReactToStepComplete(CoreStep step)
            => Members.OfType<ICanReactToStepComplete>().Any(_m => _m.CanReactToStepComplete(step));

        public bool IsFunctional => Count > 0;

        public override void ValidateGroup()
        {
        }
    }
}
