using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class Burrowing : Adjunct
    {
        public Burrowing()
            : base(typeof(BurrowMovement))
        {
        }

        public override object Clone()
            => new Burrowing();
    }
}
