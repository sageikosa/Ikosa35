using System;
using System.Collections.Generic;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Guildsmanship.Overland
{
    [Serializable]
    public abstract class ForestTerrain : Terrain
    {
        public override int AvoidLostDifficulty => 15;
        public override int MapAvoidLostDifficulty => 15;
        public override decimal HighwayMoveFactor => 1m;
        public override abstract string MaxSpotDistanceString { get; }
        public override decimal MoveFactor => 0.5m;
        public override string Name => @"Forest";
        public override decimal RoadMoveFactor => 1m;
    }

    [Serializable]
    public class SparseForestTerrain : ForestTerrain
    {
        public override string MaxSpotDistanceString => @"3d6x10";
    }

    [Serializable]
    public class MediumForestTerrain : ForestTerrain
    {
        public override string MaxSpotDistanceString => @"2d8x10";
    }

    [Serializable]
    public class DenseForestTerrain : ForestTerrain
    {
        public override string MaxSpotDistanceString => @"2d6x10";
    }
}
