using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class TroveMergeFeedback : InteractionFeedback
    {
        public TroveMergeFeedback(object source, Trove targetTrove) : base(source)
        {
            _Trove = targetTrove;
        }

        #region state
        private Trove _Trove;
        #endregion

        public Trove MergedTrove => _Trove;
    }
}
