using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Objects
{
    public interface IFlatObjectSide : IObjectBaseTacticalProperties, 
        IAnchorage, IActionProvider, ICloneable
    {
        void SetName(string name);

        /// <summary>Synonym of Length</summary>
        double Thickness { get; set; }

        string SoundDescription { get; }
    }
}
