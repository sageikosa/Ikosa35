using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// Represents potential sound as an interaction (including sound stopping)
    /// </summary>
    public class SoundData : InteractData
    {
        #region construction
        public SoundData(CoreActor actor, IAudible audible, DeltaCalcInfo difficulty, double maxRange,
            int distanceDifficulty, int effect, int roomTrans, int extra)
            : base(actor)
        {
            _Audible = audible;
            _Difficulty = difficulty;
            _MaxRange = maxRange;
            _DistanceDiff = distanceDifficulty;
            _CheckRoll = 0;
            _EffectBlock = effect;
            _RoomTrans = roomTrans;
            _Extra = extra;
            _LastAware = null;
        }
        #endregion

        #region state
        private DeltaCalcInfo _Difficulty;
        private double _MaxRange;
        private int _DistanceDiff;
        private IAudible _Audible;
        private int _CheckRoll;
        private bool? _LastAware;

        // difficulty counters
        private int _EffectBlock;
        private int _RoomTrans;
        private int _Extra;
        #endregion

        public IAudible Audible => _Audible;

        public Guid ID => _Audible.SoundGroupID;
        public DeltaCalcInfo BaseDifficulty => _Difficulty;
        public double RangeRemaining => _MaxRange;
        public int DistanceDifficulty => _DistanceDiff;
        public int EffectBlocks => _EffectBlock;
        public int RoomTransits => _RoomTrans;
        public int ExtraDifficulty => _Extra;

        /// <summary>True: aware (bonus); False: not-aware (penalty); NULL: first check</summary>
        public bool? LastAware { get => _LastAware; set => _LastAware = value; }

        /// <summary>Last check made for this audible, null if not made</summary>
        public int CheckRoll { get => _CheckRoll; set => _CheckRoll = value; }

        // TODO: cognitive load (other sound awarenesses, other awarenesses, focusing, current/recent activities)

        public string Name => _Audible.Name;

        #region public DeltaCalcInfo GetListenDifficulty()
        public DeltaCalcInfo GetListenDifficulty()
        {
            var _calc = BaseDifficulty.Copy(Audible.SoundGroupID, $@"{Name} Listen Difficulty");
            if (EffectBlocks > 0)
            {
                _calc.Result += EffectBlocks;
                _calc.AddDelta(@"Blocked Lines", EffectBlocks);
            }
            if (RoomTransits > 0)
            {
                _calc.Result += RoomTransits;
                _calc.AddDelta(@"Room Transits", RoomTransits);
            }
            if (ExtraDifficulty > 0)
            {
                _calc.Result += ExtraDifficulty;
                _calc.AddDelta(@"Extra Difficulty", ExtraDifficulty);
            }
            if (LastAware.HasValue)
            {
                if (LastAware ?? true)
                {
                    _calc.Result -= 2;
                    _calc.AddDelta(@"Continued Aware", -2);
                }
                else
                {
                    _calc.Result += 2;
                    _calc.AddDelta(@"Missed Aware", 2);
                }
            }
            _calc.Result += DistanceDifficulty;
            _calc.AddDelta(@"Distance", DistanceDifficulty);
            return _calc;
        }
        #endregion
    }
}
