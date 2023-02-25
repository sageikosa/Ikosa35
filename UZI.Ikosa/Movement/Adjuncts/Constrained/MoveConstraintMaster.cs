using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Movement.Adjuncts
{
    [Serializable]
    public class MoveConstraintMaster : GroupMasterAdjunct
    {
        public MoveConstraintMaster(object source, AdjunctGroup group)
            : base(source, group)
        {
        }

        public override object Clone()
            => new MoveConstraintMaster(Source, Group);
    }
}
