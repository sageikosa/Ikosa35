using System;
using System.Linq;
using Uzi.Ikosa.Universal;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class FlightBudget : ITickEndBudgetItem
    {
        #region construction
        protected FlightBudget(Creature critter, UpDownAdjustment upDown)
        {
            _Critter = critter;
            _Recovery = false;
            _Distance = null;   // resets every turn
            _SinceTurn = 0;     // only resets when turning
            _SinceDown = 20;    // only resets when going down
            _UpCross = upDown == UpDownAdjustment.StraightUp ? 1 : 0; // only resets when not going straight up
        }
        #endregion

        #region private data
        private Creature _Critter;
        private bool _Recovery;
        private int _UpCross;
        private double? _Distance;
        private double _SinceTurn;
        private double _SinceDown;
        private bool _Half;
        #endregion

        #region public static FlightBudget GetBudget(CoreActionBudget budget, int heading, int upCross)
        /// <summary>
        /// Gets the currently defined FlightBudget.  
        /// If none are defined, a new one is created and added to the CoreActionBudget
        /// </summary>
        public static FlightBudget GetBudget(CoreActionBudget budget, UpDownAdjustment upCross)
        {
            var _flightBudget = budget.BudgetItems[typeof(FlightBudget)] as FlightBudget;
            if (_flightBudget == null)
            {
                _flightBudget = new FlightBudget(budget.Actor as Creature, upCross);
                budget.BudgetItems.Add(_flightBudget.Source, _flightBudget);
            }
            return _flightBudget;
        }
        #endregion

        /// <summary>
        /// True if just recovered from a stalling fall, indicating that minimum flight requirements 
        /// need not be met by the end of this round to prevent a further stalling fall.
        /// </summary>
        public bool HasRecovered { get { return _Recovery; } set { _Recovery = value; } }

        /// <summary>Distance covered flying so far (not budget used)</summary>
        public double? DistanceCovered { get { return _Distance; } set { _Distance = value; } }

        /// <summary>Distance covered since the last heading change</summary>
        public double DistanceSinceTurn { get { return _SinceTurn; } set { _SinceTurn = value; } }

        /// <summary>Distance covered since the last descending move</summary>
        public double DistanceSinceDown { get { return _SinceDown; } set { _SinceDown = value; } }

        /// <summary>Number of sequential straight upwards crossings</summary>
        public int UpwardsCrossings { get { return _UpCross; } set { _UpCross = value; } }

        /// <summary>Only need to maintain half of minimum speed to avoid stall</summary>
        public bool HalfSpeed { get { return _Half; } set { _Half = value; } }

        #region IBudgetItem Members

        public object Source => typeof(FlightBudget);
        public string Name => @"Flight Movement Budget";
        public string Description => @"Tracks and maintains flight maneuverability and consequences";
        public void Added(Core.CoreActionBudget budget) { }
        public void Removed() { }

        #endregion

        #region ITickEndBudgetItem Members

        public bool EndTick()
        {
            // if creature has just recovered from a stall, do not try to stall again
            // creature must be in flight, and not perched
            if (!HasRecovered && _Critter.Adjuncts.OfType<InFlight>().Any(_if => !_if.IsPerched))
            {
                // minimum needed for a given speed for non-good/perfect maneuverabilities
                double _minSpeed(double speed)
                    => speed / (HalfSpeed ? 4 : 2);

                // check for plummet if minimums not met (check across all usable flight movements)
                if (_Critter.Movements.AllMovements.OfType<FlightSuMovement>().Where(_f => _f.IsUsable)
                    .All(_f => (_f.ManeuverabilityRating > FlightManeuverability.Good)
                        && ((DistanceCovered ?? 0) < _minSpeed(_f.BaseDoubleValue))))
                {
                    // none of the usable flights had perfect/good rating
                    // ... and all usable flights failed to meet minimum speed
                    var _loc = Locator.FindFirstLocator(_Critter);
                    if (_loc?.IsGravityEffective ?? false)
                    {
                        FallingStartStep.StartFall(_loc, 150, 300, @"Flight Stall");
                    }
                }
            }

            DistanceCovered = null;
            HasRecovered = false;

            // do not remove (still need continuous information on heading, up, down and turning)
            return false;
        }

        #endregion

        public FlightBudgetInfo ToFlightBudgetInfo()
        {
            var _info = this.ToBudgetItemInfo<FlightBudgetInfo>();
            _info.UpwardsCrossings = UpwardsCrossings;
            _info.DistanceCovered = DistanceCovered ?? 0;
            _info.DistanceSinceTurn = DistanceSinceTurn;
            return _info;
        }
    }
}
