using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Movement.Adjuncts
{
    [Serializable]
    public class MoveConstraintGroup : AdjunctGroup
    {
        public MoveConstraintGroup(object source)
            : base(source)
        {
        }

        public MoveConstraintMaster MoveConstraintMaster
            => Members.OfType<MoveConstraintMaster>().FirstOrDefault();

        public IEnumerable<MoveConstraintEndPoint> Constrained
            => Members.OfType<MoveConstraintEndPoint>();

        public override void ValidateGroup()
            => this.ValidateOneToManyPlanarGroup();
    }
}
