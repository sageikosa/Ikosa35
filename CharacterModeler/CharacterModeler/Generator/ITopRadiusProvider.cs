using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CharacterModeler.Generator
{
    public interface ITopRadiusProvider : ITopDimensionProvider
    {
        double Radius { get; }
    }
}
