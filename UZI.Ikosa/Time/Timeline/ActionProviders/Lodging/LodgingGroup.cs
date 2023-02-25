using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class LodgingGroup : AdjunctGroup
    {
        public LodgingGroup(object source)
            : base(source)
        {
        }

        public Lodging Lodging => Members.OfType<Lodging>().FirstOrDefault();
        public List<Lodger> Lodgers => Members.OfType<Lodger>().ToList();

        public override void ValidateGroup()
            => this.ValidateOneToManyPlanarGroup();
    }
}
