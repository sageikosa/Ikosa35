using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Guildsmanship.Overland
{
    [Serializable]
    public abstract class SandyTerrain : Terrain
    {
        public override int AvoidLostDifficulty => 4;
        public override int MapAvoidLostDifficulty => 0;
        public override decimal HighwayMoveFactor => 1m;
        public override abstract string MaxSpotDistanceString { get; }
        public override decimal MoveFactor => 0.5m;
        public override string Name => @"Sandy";
        public override decimal RoadMoveFactor => 0.5m;
    }

    [Serializable]
    public class RockyDesert : SandyTerrain
    {
        public override string MaxSpotDistanceString => @"6d6x20";
    }

    [Serializable]
    public class DuneDesert : SandyTerrain
    {
        public override string MaxSpotDistanceString => @"6d6x10";
    }
}
