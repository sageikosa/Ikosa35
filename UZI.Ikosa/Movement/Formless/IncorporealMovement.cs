using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class IncorporealMovement : FlightSuMovement
    {
        public IncorporealMovement(int speed, CoreObject coreObj, object source, FlightManeuverability flightManeuverability) 
            : base(speed, coreObj, source, flightManeuverability, false, true)
        {
        }
    }
}
