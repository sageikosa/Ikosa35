using System;
using System.Linq;

namespace Uzi.Core
{
    /// <summary>
    /// Adjunct group that allows members of starting processes to react to the processes by side effects
    /// </summary>
    [Serializable]
    public class ReactorSideEffectGroup : AdjunctGroup, ICanReactBySideEffect
    {
        public ReactorSideEffectGroup(object source)
            : base(source)
        {
        }

        #region ICanReact Members

        public void ReactToProcessBySideEffect(CoreProcess process)
        {
            foreach (var _reactor in Members.OfType<ICanReactBySideEffect>())
            {
                _reactor.ReactToProcessBySideEffect(process);
            }
        }

        public bool IsFunctional => Count > 0;

        #endregion

        public override void ValidateGroup()
        {
        }
    }
}
