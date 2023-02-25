using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Guildsmanship.Overland
{
    [Serializable]
    public class MountainTerrain : Terrain
    {
        public override int AvoidLostDifficulty => 12;
        public override int MapAvoidLostDifficulty => 8;
        public override decimal HighwayMoveFactor => 0.75m;
        public override string MaxSpotDistanceString => @"4d10x10";
        public override decimal MoveFactor => 0.5m;
        public override string Name => @"Mountain";
        public override decimal RoadMoveFactor => 0.75m;
    }
}
