using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CharacterModeler.Generator
{
    public interface IBottomRadiusProvider : IBottomDimensionProvider
    {
        double Radius { get; }
    }
}
