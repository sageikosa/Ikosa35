using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class GaseousMovement : FlightSuMovement
    {
        public GaseousMovement(int speed, CoreObject coreObj, object source, FlightManeuverability flightManeuverability) 
            : base(speed, coreObj, source, flightManeuverability, false, true)
        {
        }
    }
}
