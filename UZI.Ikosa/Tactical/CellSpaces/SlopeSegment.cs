using System;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Tactical
{
    public struct SlopeSegment
    {
        /// <summary>Low elevation</summary>
        public double Low { get; set; }

        /// <summary>High Elevation</summary>
        public double High { get; set; }

        /// <summary>Run of Segment</summary>
        public double Run { get; set; }

        public IMoveAlterer Source { get; set; }

        public double Middle()
        {
            return (High + Low) / 2;
        }

        public double Incline()
        {
            return Math.Asin((High - Low) / Run);
        }
    }
}
