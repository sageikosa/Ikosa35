using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Take10Check : Adjunct
    {
        public Take10Check(Type checkType)
            : base(checkType)
        {
        }

        public Type SourceType { get { return Source as Type; } }

        public override object Clone()
        {
            return new Take10Check(SourceType);
        }
    }

    public static class Take10CheckStatic
    {
        /// <summary>
        /// True if critter has a take 10 check setting that will apply to this type
        /// </summary>
        public static bool IsTake10InEffect(this Creature critter, Type testType)
        {
            // NOTE: using IsAssignableFrom as it return true for base class SourceTypes also
            return critter.Adjuncts.OfType<Take10Check>()
                .Any(_t => _t.IsActive && _t.SourceType.IsAssignableFrom(testType));
        }

        /// <summary>
        /// Get time remaining for the specified take 10 setting
        /// </summary>
        public static double? GetTake10Remaining(this Creature critter, Type testType)
        {
            var _take10 = critter.Adjuncts.OfType<Take10Check>()
                .FirstOrDefault(_t => _t.IsActive && _t.SourceType == testType);
            if (_take10 != null)
            {
                var _expiry = critter.Adjuncts.OfType<Expiry>()
                    .Where(_e => _e.ExpirableAdjuncts.Any(_xa => _xa == _take10))
                    .Select(_e => (double?)_e.EndTime)
                    .FirstOrDefault();
                if (_expiry.HasValue)
                {
                    return _expiry - (critter?.GetCurrentTime() ?? 0);
                }
            }
            return null;
        }

        /// <summary>
        /// Sets the time for the take 10 option
        /// </summary>
        public static void SetTake10Duration(this Creature critter, Type testType, double duration)
        {
            // find any mathing take 10
            var _take10 = critter.Adjuncts.OfType<Take10Check>()
               .FirstOrDefault(_t => _t.IsActive && _t.SourceType == testType);
            if (_take10 != null)
            {
                // remove
                var _expiry = critter.Adjuncts.OfType<Expiry>()
                    .Where(_e => _e.ExpirableAdjuncts.Any(_xa => _xa == _take10))
                    .FirstOrDefault();
                if (_expiry != null)
                {
                    _expiry.Eject();
                }
                else
                {
                    _take10.Eject();
                }
            }

            // add if necessary
            if (duration > 0)
            {
                _take10 ??= new Take10Check(testType);
                var _expiry = new Expiry(_take10, (critter?.GetCurrentTime() ?? double.MaxValue) + duration, TimeValTransition.Entering, Round.UnitFactor);
                critter.AddAdjunct(_expiry);
            }
        }
    }
}
