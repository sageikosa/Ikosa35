using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharacterModeler.Generator
{
    public class HandCrossbow : Crossbow
    {
        public HandCrossbow()
            : base()
        {
            BarrelLength = 1.25;
            BarrelWidth = 0.12;
            BowLength = 1;
            BowWidth = 0.75;
            DrawBackFactor = 1.25;
        }
    }
}
