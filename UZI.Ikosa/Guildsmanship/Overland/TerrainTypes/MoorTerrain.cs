using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Guildsmanship.Overland
{
    [Serializable]
    public class MoorTerrain : Terrain
    {
        public override int AvoidLostDifficulty => 10;
        public override int MapAvoidLostDifficulty => 6;
        public override decimal HighwayMoveFactor => 1m;
        public override string MaxSpotDistanceString => @"6d6x10";
        public override decimal MoveFactor => 0.75m;
        public override string Name => @"Moor";
        public override decimal RoadMoveFactor => 1m;
    }
}
