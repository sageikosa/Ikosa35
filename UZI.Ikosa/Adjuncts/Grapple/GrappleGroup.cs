using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class GrappleGroup : AdjunctGroup
    {
        public GrappleGroup(object source) 
            : base(source)
        {
        }

        public IEnumerable<Grappler> Grapplers => Members.OfType<Grappler>();  

        public override void ValidateGroup()
        {
            this.ValidateParticipantsPlanarGroup();
        }
    }
}
