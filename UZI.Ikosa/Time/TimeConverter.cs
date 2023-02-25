using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Time
{
    public static class TimeConverter
    {
        public static (double number, TimeUnit timeUnit) GetBestExpression(double duration)
        {
            (double quantity, TimeUnit unit) _parse<T>(double unitFactor, double floorVal) where T : TimeUnit, new()
            {
                var _quantity = duration / unitFactor;
                if ((_quantity >= 1) && ((duration % unitFactor) == 0)
                    || (_quantity >= floorVal))
                {
                    return (Math.Floor(_quantity), new T());
                }
                return (0, null);
            }


            // years?
            var _years = _parse<Year>(Year.UnitFactor, 3);
            if (_years.unit != null) return _years;

            // months?
            var _months = _parse<Month>(Month.UnitFactor, 3);
            if (_months.unit != null) return _months;

            // weeks?
            var _weeks = _parse<Week>(Week.UnitFactor, 3);
            if (_weeks.unit != null) return _weeks;

            // days?
            var _days = _parse<Day>(Day.UnitFactor, 3);
            if (_days.unit != null) return _days;

            // hours?
            var _hours = _parse<Hour>(Hour.UnitFactor, 3);
            if (_hours.unit != null) return _hours;

            // minute?
            var _minutes = _parse<Minute>(Minute.UnitFactor, 3);
            if (_minutes.unit != null) return _minutes;

            // rounds?
            return (Math.Floor(duration / Round.UnitFactor), new Round());
        }
    }
}
