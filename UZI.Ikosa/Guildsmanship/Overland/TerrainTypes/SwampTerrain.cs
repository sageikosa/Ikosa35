using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Guildsmanship.Overland
{
    [Serializable]
    public class SwampTerrain :Terrain
    {
        public override int AvoidLostDifficulty => 10;
        public override int MapAvoidLostDifficulty => 6;
        public override decimal HighwayMoveFactor => 1m;
        public override string MaxSpotDistanceString => @"2d8x10";
        public override decimal MoveFactor => 0.5m;
        public override string Name => @"Swamp";
        public override decimal RoadMoveFactor => 0.75m;
    }
}
