using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Guildsmanship.Overland
{
    [Serializable]
    public abstract class Terrain : ITerrain
    {
        #region data
        private Dictionary<byte, TileRef> _Tiles;
        #endregion

        protected Terrain()
        {
            _Tiles = [];
        }

        public abstract string Name { get; }
        public abstract decimal MoveFactor { get; }
        public abstract decimal RoadMoveFactor { get; }
        public abstract decimal HighwayMoveFactor { get; }
        public abstract int AvoidLostDifficulty { get; }
        public abstract int MapAvoidLostDifficulty { get; }
        public abstract string MaxSpotDistanceString { get; }

        public Dictionary<byte, TileRef> Tiles => _Tiles;

        public TileRef GetTileRef(byte index)
            => _Tiles.TryGetValue(index, out var _tile)
            ? _tile
            : null;

        public bool HasTileRef(byte index)
            => _Tiles.ContainsKey(index);

        public decimal MaxSpotDistance()
            => Convert.ToDecimal(new ComplexDiceRoller(MaxSpotDistanceString).RollValue(Guid.Empty, @"Terrain", @"Spot Distance"));
    }
}
