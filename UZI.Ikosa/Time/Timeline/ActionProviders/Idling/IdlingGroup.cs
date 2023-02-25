using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class IdlingGroup : AdjunctGroup
    {
        public IdlingGroup(object source)
            : base(source)
        {
        }

        public List<IdlingActor> Actors => Members.OfType<IdlingActor>().ToList();
        public override void ValidateGroup() { }
    }
}
