using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CharacterModeler.Generator
{
    public class ShortBow : DrawnBow
    {
        public ShortBow()
        {
            DrawBackFactor = 1.5d;
            Length = 3;
            Width = 0.66;
        }
    }
}
