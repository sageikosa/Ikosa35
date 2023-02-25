using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Guildsmanship.Overland
{
    [Serializable]
    public abstract class HillTerrain : Terrain
    {
        public override int AvoidLostDifficulty => 10;
        public override int MapAvoidLostDifficulty => 6;
        public override decimal HighwayMoveFactor => 1m;
        public override abstract string MaxSpotDistanceString { get; }
        public override decimal MoveFactor => 0.5m;
        public override string Name => @"Hill";
        public override decimal RoadMoveFactor => 0.75m;
    }

    [Serializable]
    public class GentleHillTerrain : HillTerrain
    {
        public override string MaxSpotDistanceString => @"2d10x10";
    }

    [Serializable]
    public class RuggedHillTerrain : HillTerrain
    {
        public override string MaxSpotDistanceString => @"2d6x10";
    }
}
