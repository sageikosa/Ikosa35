using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Time
{
    /// <summary>Anchored to a CampSiteFocus for an proposed camp location</summary>
    [Serializable]
    public class DecideCampSite : GroupMasterAdjunct
    {
        public DecideCampSite(DecideCampGroup camping)
            : base(camping, camping)
        {
        }

        public override object Clone()
            => new DecideCampSite(DecideCampGroup);

        public DecideCampGroup DecideCampGroup => Group as DecideCampGroup;
        public CampSiteFocus CampSiteFocus => Anchor as CampSiteFocus;

        protected override void OnDeactivate(object source)
        {
            // if a camp hasn't been setup, abandon the site, the decision failed
            if (!CampSiteFocus.HasAdjunct<CampSite>())
            {
                CampSiteFocus.Abandon();
            }

            base.OnDeactivate(source);
        }
    }
}
