using System;
using System.Collections.Generic;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Guildsmanship.Overland
{
    [Serializable]
    public class CliffTerrain : Terrain
    {
        public override int AvoidLostDifficulty => 12;
        public override int MapAvoidLostDifficulty => 8;
        public override decimal HighwayMoveFactor => 0.5m;
        public override string MaxSpotDistanceString => @"4d10x10";
        public override decimal MoveFactor => 0.0m;
        public override string Name => @"Cliff";
        public override decimal RoadMoveFactor => 0.25m;
    }
}
