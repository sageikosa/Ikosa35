using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CharacterModeler.Generator
{
    public class LightCrossbow : Crossbow
    {
        public LightCrossbow()
            : base()
        {
            BarrelLength = 2.5;
            BarrelWidth = 0.16;
            BowLength = 2;
            BowWidth = 0.75;
            DrawBackFactor = 1.5;
        }
    }
}
