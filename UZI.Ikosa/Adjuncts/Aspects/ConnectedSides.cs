using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class ConnectedSides : Adjunct
    {
        public ConnectedSides(params SideIndex[] sides)
            : base(typeof(ConnectedSides))
        {
            _Sides = sides;
        }

        private SideIndex[] _Sides;

        public SideIndex[] Sides => _Sides;

        public override object Clone()
            => new ConnectedSides(Sides);
    }
}
