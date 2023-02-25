using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Time
{
    /// <summary>Anchored to a CampSiteFocus for an active camp location</summary>
    [Serializable]
    public class CampSite : GroupMasterAdjunct
    {
        public CampSite(CampingGroup camping)
            : base(camping, camping)
        {
        }

        public override object Clone()
            => new CampSite(CampingGroup);

        public CampingGroup CampingGroup => Group as CampingGroup;
        public CampSiteFocus CampSiteFocus => Anchor as CampSiteFocus;

        protected override void OnDeactivate(object source)
        {
            // ejecting the camp site removes its final reason to be
            CampSiteFocus.Abandon();
            base.OnDeactivate(source);
        }
    }
}
