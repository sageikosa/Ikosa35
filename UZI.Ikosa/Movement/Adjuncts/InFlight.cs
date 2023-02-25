using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Movement
{
    /// <summary>Indicates the creature is considered to be in-flight (using Fly movement, and not supported by gravity)</summary>
    [Serializable]
    public class InFlight : Adjunct, ITrackTime
    {
        /// <summary>Indicates the creature is considered to be in-flight (using Fly movement, and not supported by gravity)</summary>
        public InFlight()
            : base(typeof(FlightSuMovement))
        {
            _IsPerched = false;
            _NextTime = 0;
        }

        private bool _IsPerched;
        private double _NextTime;

        /// <summary>Set to true if perched (supported by gravity)</summary>
        public bool IsPerched { get { return _IsPerched; } set { _IsPerched = value; } }

        /// <summary>Time value at which to determine that creature is in-flight at start of turn and initialize DistanceCovered</summary>
        public double NextTime { get { return _NextTime; } set { _NextTime = value; } }

        public override object Clone()
        {
            return new InFlight();
        }

        #region ITrackTime Members

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            // if not supported against gravity
            if (!IsPerched && (direction == TimeValTransition.Entering))
            {
                if (Anchor is Creature _critter)
                {
                    // if all usable flights are worse than good maneuverability
                    if (_critter.Movements.AllMovements.OfType<FlightSuMovement>().Where(_f => _f.IsUsable)
                        .All(_f => _f.ManeuverabilityRating > FlightManeuverability.Good))
                    {
                        // get budget for creature
                        var _budget = (_critter.ProcessManager as IkosaProcessManager)
                            ?.LocalTurnTracker.ReactableBudgets.FirstOrDefault(_b => _b.Actor == _critter);
                        if (_budget != null)
                        {
                            // get flight budget for creature
                            if (_budget.BudgetItems[typeof(FlightBudget)] is FlightBudget _flightBudget)
                            {
                                // start at 0 (rather than null)
                                _flightBudget.DistanceCovered = 0;
                            }
                        }
                    }
                }
            }
        }

        public double Resolution { get { return Round.UnitFactor; } }

        #endregion
    }
}
