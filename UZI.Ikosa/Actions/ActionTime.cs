using System;
using Uzi.Core.Dice;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa
{
    /// <summary>
    /// Indicates the time needed to perform an action, or the length of an adjunct
    /// </summary>
    [Serializable]
    public class ActionTime
    {
        #region Construction
        public ActionTime(TimeType actionType)
        {
            ActionTimeType = actionType;
            if (actionType.Equals(TimeType.Span))
                _Span = 1;
            else
                _Span = 0;
        }

        public ActionTime(double spanLength)
        {
            if (spanLength > 0)
            {
                ActionTimeType = TimeType.Span;
                _Span = spanLength;
            }
            else
            {
                ActionTimeType = TimeType.Regular;
                _Span = 0;
            }
        }

        public ActionTime(double spanLength, TimeType timeType)
        {
            if (spanLength > 0)
            {
                ActionTimeType = timeType;
                _Span = spanLength;
            }
            else
            {
                ActionTimeType = TimeType.Regular;
                _Span = 0;
            }
        }

        public ActionTime(TimeUnit unit, Roller roll, TimeType timeType)
        {
            _Unit = unit ?? new Round();
            ActionTimeType = timeType;
            _Span = -1;
            _Roll = roll ?? new DieRoller(4);
        }
        #endregion

        #region state
        private double _Span;
        private readonly Roller _Roll;
        private readonly TimeUnit _Unit;
        #endregion

        #region public static bool operator >(ActionTime first, ActionTime second)
        public static bool operator >(ActionTime first, ActionTime second)
        {
            switch (first.ActionTimeType)
            {
                case TimeType.TimelineScheduling:
                case TimeType.Span:
                    if ((second.ActionTimeType == TimeType.Span)
                        || (second.ActionTimeType == TimeType.TimelineScheduling))
                    {
                        return first.CompareSpan() > second.CompareSpan();
                    }
                    return true;

                default:
                    return ((int)first.ActionTimeType) > ((int)second.ActionTimeType);
            }
        }
        #endregion

        #region public static bool operator <(ActionTime second, ActionTime first)
        public static bool operator <(ActionTime second, ActionTime first)
        {
            switch (first.ActionTimeType)
            {
                case TimeType.TimelineScheduling:
                case TimeType.Span:
                    if ((second.ActionTimeType == TimeType.Span)
                        || (second.ActionTimeType == TimeType.TimelineScheduling))
                    {
                        return first.CompareSpan() > second.CompareSpan();
                    }
                    return true;

                default:
                    return ((int)first.ActionTimeType) > ((int)second.ActionTimeType);
            }
        }
        #endregion

        /// <summary>Does not trigger random roll resolution</summary>
        private double CompareSpan()
            => _Span < 0 ? 1 : _Span;

        public readonly TimeType ActionTimeType;

        #region public double SpanLength { get; }
        public double SpanLength
        {
            get
            {
                if (_Span < 0)
                {
                    _Span = (_Unit?.BaseUnitFactor ?? 1) * (_Roll?.RollValue(Guid.Empty, @"Random Time Span", _Unit.PluralName) ?? 1);
                }
                return _Span;
            }
        }
        #endregion

        #region public bool IsBoundToTurnTick { get; }
        /// <summary>
        /// True if the time cost requires the action budget to bind to the tick for future turns. 
        /// <para>TRUE for reactive, brief, regular, total and span.</para>
        /// <para>FALSE for free, opportunistic, choice, twitch and sub-action.</para>
        /// </summary>
        public bool IsBoundToTurnTick
        {
            get
            {
                switch (ActionTimeType)
                {
                    case TimeType.Reactive:
                    case TimeType.Brief:
                    case TimeType.Regular:
                    case TimeType.Total:
                    case TimeType.Span:
                    case TimeType.TimelineScheduling:
                        return true;
                    default:
                        return false;
                }
            }
        }
        #endregion

        public override string ToString()
            => (ActionTimeType == TimeType.Span)
            ? (_Span < 0 ? @"random time" : $@"{SpanLength} rounds")
            : ActionTimeType.ToString();
    }
}
