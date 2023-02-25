using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Guildsmanship.Overland
{
    [Serializable]
    public class CustomTerrain : Terrain
    {
        public CustomTerrain(string name, decimal move, decimal roadMove, decimal highwayMove,
            int lostDifficulty, int mapLostDifficulty, string maxSpot)
        {
            _Name = name;
            _MoveFactor = move;
            _RoadMove = roadMove;
            _HighwayMove = highwayMove;
            _Lost = lostDifficulty;
            _MapLost = mapLostDifficulty;
            _MaxSpot = maxSpot;
        }

        #region data
        private string _Name;
        private decimal _MoveFactor;
        private decimal _RoadMove;
        private decimal _HighwayMove;
        private int _Lost;
        private int _MapLost;
        private string _MaxSpot;
        #endregion

        public override string Name => _Name;
        public override decimal MoveFactor => _MoveFactor;
        public override decimal RoadMoveFactor => _RoadMove;
        public override decimal HighwayMoveFactor => _HighwayMove;
        public override int AvoidLostDifficulty => _Lost;
        public override int MapAvoidLostDifficulty => _MapLost;
        public override string MaxSpotDistanceString => _MaxSpot;
    }
}
