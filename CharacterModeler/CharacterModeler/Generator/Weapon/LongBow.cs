using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CharacterModeler.Generator
{
    public class LongBow : DrawnBow
    {
        public LongBow()
        {
            DrawBackFactor = 1d;
            Length = 5;
            Width = 0.75;
        }
    }
}
