using System;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Movement
{
    /// <summary>Extraordinary Flight (derived from FlightSuMovement)</summary>
    [Serializable]
    public class FlightExMovement : FlightSuMovement
    {
        /// <summary>Extraordinary Flight (derived from FlightSuMovement)</summary>
        public FlightExMovement(int speed, Creature creature, object source, FlightManeuverability flightManeuverability)
            : this(speed, creature, source, flightManeuverability, false, false)
        {
        }

        /// <summary>Extraordinary Flight (derived from FlightSuMovement)</summary>
        public FlightExMovement(int speed, Creature creature, object source, FlightManeuverability flightManeuverability,
            bool armorEncumberance, bool heavyAsMedium)
            : base(speed, creature, source, flightManeuverability, armorEncumberance, heavyAsMedium)
        {
        }

        public override string FlightType => @"Ex";
        public override string Name => @"Fly";

        public override bool CanMoveThrough(CellMaterial material)
        {
            // extra-ordinary flight only works in a gaseous medium (not void)
            return material is GasCellMaterial;
        }

        public override MovementBase Clone(Creature forCreature, object source)
            => new FlightExMovement(BaseValue, forCreature, source, ManeuverabilityRating, _ArmorEncumberance, _HeavyAsMedium);
    }
}
