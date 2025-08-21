using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class PreRelocateFeedback : InteractionFeedback
    {
        public PreRelocateFeedback(object source)
            : base(source)
        {
            _ReactiveSteps = [];
        }

        #region data
        private List<CoreStep> _ReactiveSteps;
        #endregion

        public List<CoreStep> ReactiveSteps => _ReactiveSteps;
    }
}
