using System;
using System.Collections.Generic;

namespace Uzi.Ikosa.Guildsmanship.Overland
{
    [Serializable]
    public class FrozenTerrain : Terrain
    {
        public override int AvoidLostDifficulty => 4;
        public override int MapAvoidLostDifficulty => 0;
        public override decimal HighwayMoveFactor => 1m;
        public override string MaxSpotDistanceString => @"6d6x20";
        public override decimal MoveFactor => 0.75m;
        public override string Name => @"Frozen";
        public override decimal RoadMoveFactor => 0.75m;
    }
}
