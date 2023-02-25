using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class LocalTurnTick : CoreTurnTick
    {
        #region ctor()
        /// <summary>Creates the tick, and adds it either as the first, or last tick</summary>
        private LocalTurnTick(double time, LocalTurnTracker tracker, int? initiativeScore, bool addToTail, bool isRoundMarker)
            : base(tracker, addToTail)
        {
            _Time = time;
            _InitScore = initiativeScore;
            _RoundMarker = isRoundMarker;
        }

        /// <summary>Creates the tick, and adds if before or after the selected node.</summary>
        public LocalTurnTick(double time, LocalTurnTracker tracker, int? initiativeScore, LinkedListNode<CoreTurnTick> addBefore)
            : base(tracker, addBefore)
        {
            _Time = time;
            _InitScore = initiativeScore;
            _RoundMarker = false;
        }
        #endregion

        public static LocalTurnTick CreateNewLeadTick(LocalTurnTracker tracker)
            => new LocalTurnTick(tracker.Map.CurrentTime + tracker.TimeDelta(1), tracker, null, false, false);

        public static LocalTurnTick CreateInitiativeStartupTick(double time, LocalTurnTracker tracker, int initiative)
            => new LocalTurnTick(time, tracker, initiative, true, false);

        public static LocalTurnTick CreateRoundMarker(LocalTurnTracker tracker)
            => new LocalTurnTick(tracker.Map.CurrentTime + tracker.TickResolution, tracker, null, true, true);

        #region data
        private double _Time;
        private int? _InitScore;
        private bool _RoundMarker;
        #endregion

        public int? InitiativeScore => _InitScore;

        public double Time
        {
            get
            {
                if (!TurnTracker.IsInitiative)
                {
                    var _currentTime = TurnTracker.Map.CurrentTime;
                    if (_currentTime > _Time)
                    {
                        var _tickRes = TurnTracker.TickResolution;
                        _Time = Math.Ceiling(_currentTime + (_tickRes - (_currentTime % _tickRes)));
                    }
                }
                return _Time;
            }
        }

        public LocalTurnTracker TurnTracker => (Tracker as LocalTurnTracker);

        /// <summary>True if this tick marks the end of the round (and thus the start of the next round)</summary>
        public bool IsRoundMarker => _RoundMarker || !TurnTracker.IsInitiative;

        /// <summary>Ticks the clock, updates the next time and moves the tick to the end of the Ticks list.</summary>
        public override void EndOfTick()
        {
            _Time += TurnTracker.TickResolution;
            base.EndOfTick();
        }
    }
}
