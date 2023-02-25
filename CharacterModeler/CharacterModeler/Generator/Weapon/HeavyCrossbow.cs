using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharacterModeler.Generator
{
    public class HeavyCrossbow : Crossbow
    {
        public HeavyCrossbow()
            : base()
        {
            BarrelLength = 3;
            BarrelWidth = 0.2;
            BowLength = 2.5;
            BowWidth = 0.8;
            DrawBackFactor = 1.6;
        }
    }
}
