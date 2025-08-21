using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class RemoteSenseGroup : AdjunctGroup
    {
        public RemoteSenseGroup(object source, bool coPlanar)
            : base(source)
        {
            _CoPlanar = coPlanar;
        }

        #region state
        private bool _CoPlanar;
        #endregion

        public RemoteSenseMaster Master => Members.OfType<RemoteSenseMaster>().FirstOrDefault();
        public RemoteSenseTarget Target => Members.OfType<RemoteSenseTarget>().FirstOrDefault();
        public bool MustBeCoPlanar => _CoPlanar;

        public override void ValidateGroup()
        {
            if (_CoPlanar)
            {
                this.ValidateOneToOnePlanarGroup();
            }
        }
    }
}
