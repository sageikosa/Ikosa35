using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Guildsmanship.Overland
{
    [Serializable]
    public class JungleTerrain : Terrain
    {
        public override int AvoidLostDifficulty => 20;
        public override int MapAvoidLostDifficulty => 20;
        public override decimal HighwayMoveFactor => 1m;
        public override string MaxSpotDistanceString => @"2d6x10";
        public override decimal MoveFactor => 0.25m;
        public override string Name => @"Jungle";
        public override decimal RoadMoveFactor => 0.75m;
    }
}
