using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Guildsmanship.Overland
{
    [Serializable]
    public class PlainTerrain : Terrain
    {
        public override int AvoidLostDifficulty => 2;
        public override int MapAvoidLostDifficulty => 0;
        public override decimal HighwayMoveFactor => 1m;
        public override string MaxSpotDistanceString => @"6d6x40";
        public override decimal MoveFactor => 0.75m;
        public override string Name => @"Plain";
        public override decimal RoadMoveFactor => 1m;
    }
}
