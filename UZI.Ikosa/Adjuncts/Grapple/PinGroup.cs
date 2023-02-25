using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class PinGroup : AdjunctGroup
    {
        public PinGroup(object source)
            : base(source)
        {
        }

        public Pinner Pinner => Members.OfType<Pinner>().FirstOrDefault();
        public Pinnee Pinnee => Members.OfType<Pinnee>().FirstOrDefault();

        public override void ValidateGroup()
        {
            this.ValidateOneToOnePlanarGroup();
        }
    }
}
