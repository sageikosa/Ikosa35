using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class SoundRef
    {
        #region ctor()
        /// <summary>Cancellation</summary>
        public SoundRef(IAudible audible, ulong serial)
        {
            _Audible = audible;
            _Difficulty = new DeltaCalcInfo(Guid.Empty, @"Cancel");
            _SourceRange = 0;
            _Range = 0;
            _Distance = 0;
            _EffectBlock = 0;
            _RoomTrans = 0;
            _Extra = 0;
            _Serial = serial;
        }

        /// <summary>Initial</summary>
        public SoundRef(IAudible audible, DeltaCalcInfo difficulty, double sourceRange, ulong serial)
        {
            _Audible = audible;
            _Difficulty = difficulty;
            _SourceRange = sourceRange;
            _Range = sourceRange;
            _Distance = 0;
            _EffectBlock = 0;
            _RoomTrans = 0;
            _Extra = 0;
            _Serial = serial;
        }

        /// <summary>Initial</summary>
        public SoundRef(IAudible audible, int difficulty, double sourceRange, ulong serial)
        {
            _Audible = audible;
            _Difficulty = new DeltaCalcInfo(audible.SourceID, @"Listen Difficulty") { BaseValue = difficulty, Result = difficulty };
            _SourceRange = sourceRange;
            _Range = sourceRange;
            _Distance = 0;
            _EffectBlock = 0;
            _RoomTrans = 0;
            _Extra = 0;
            _Serial = serial;
        }

        /// <summary>Transmission</summary>
        public SoundRef(SoundRef seed, DeltaCalcInfo difficulty, double maxRangeRemaining, int plusDistance,
            int plusEffect, int plusRoomTrans, int plusExtra, ulong serial, ICellLocation cell)
        {
            _Audible = seed.Audible;
            _SourceRange = seed.SourceRange;
            _Difficulty = difficulty;
            _Range = maxRangeRemaining;

            _Distance = seed.DistanceDifficulty + plusDistance;
            _EffectBlock = seed.EffectBlocks + plusEffect;
            _RoomTrans = seed.RoomTransits + plusRoomTrans;
            _Extra = seed.ExtraDifficulty + plusExtra;

            _Serial = serial;
            _Cell = cell != null ? new CellLocation(cell) : null;
        }
        #endregion

        #region state
        private IAudible _Audible;
        private DeltaCalcInfo _Difficulty;
        private double _SourceRange;
        private double _Range;

        // difficulty counters
        private int _Distance;
        private int _EffectBlock;
        private int _RoomTrans;
        private int _Extra;
        private CellLocation _Cell;

        [NonSerialized, JsonIgnore]
        private ulong _Serial;
        #endregion

        public IAudible Audible => _Audible;
        public double SourceRange => _SourceRange;
        public double RangeRemaining => _Range;
        public CellLocation Cell => _Cell;

        public DeltaCalcInfo BaseDifficulty => _Difficulty;
        public int DistanceDifficulty => _Distance;
        public int EffectBlocks => _EffectBlock;
        public int RoomTransits => _RoomTrans;
        public int ExtraDifficulty => _Extra;

        public int TotalDifficulty => _Difficulty.Result + _Distance + _EffectBlock + _RoomTrans + _Extra;

        public ulong SerialState => _Serial;

        public SoundRef GetRefresh(ulong serial)
            => new SoundRef(this, _Difficulty, _Range, 0, 0, 0, 0, serial, _Cell);

        public double GetRelativeMagnitude(IGeometricRegion source, IGeometricContext target)
            => RangeRemaining - source.NearDistance(target.GeometricRegion);

        #region public SoundData GetTransit(IGeometricRegion source, IGeometricContext target)
        /// <summary>Get a sound data that has already moved through the environment from source to target</summary>
        public SoundData GetTransit(IGeometricRegion source, IGeometricContext target)
        {
            // get distance from sound source to target ctx
            var _distance = source.NearDistance(target.GeometricRegion);

            // calculate values for sound reference, if max range drops below zero, no need to track this sound
            var _range = Math.Max(RangeRemaining - _distance, 0);
            if (_range > 0)
            {
                var _distanceDiff = Convert.ToInt32(Math.Ceiling(_distance / 10d));
                var _map = target.MapContext.Map;
                var _audible = _map.GetICore<ICore>(Audible.SourceID);

                // one line per each cell-cell
                var _factory = new SegmentSetFactory(target.MapContext.Map, source, target.GeometricRegion,
                                ITacticalInquiryHelper.EmptyArray, SegmentSetProcess.Geometry);
                foreach (var _sCell in source.AllCellLocations())
                {
                    foreach (var _tCell in target.GeometricRegion.AllCellLocations())
                    {
                        var _line = _map.SegmentCells(_sCell.GetPoint(), _tCell.GetPoint(), _factory, PlanarPresence.Both);
                        var _lineRange = _range - Math.Max(_line.IsLineOfEffect ? 0 : 10, 0);
                        if (_lineRange > 0)
                        {
                            var _soundTrans = new SoundTransit(Audible);
                            var _interact = new Interaction(null, _audible, null, _soundTrans);
                            if (_line.CarryInteraction(_interact))
                            {
                                return new SoundData(null, Audible, BaseDifficulty, _lineRange, DistanceDifficulty + _distanceDiff,
                                    EffectBlocks + (_line.IsLineOfEffect ? 0 : 1), RoomTransits,
                                    ExtraDifficulty + _soundTrans.AddedDifficulty.EffectiveValue);
                            }
                        }
                    }
                }
            }

            return null;
        }
        #endregion
    }
}
