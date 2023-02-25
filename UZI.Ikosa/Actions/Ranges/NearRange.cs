using System;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// 25 feet + 5 feet per 2 caster levels
    /// </summary>
    [Serializable]
    public class NearRange : Range
    {
        /// <summary>
        /// 25 feet + 5 feet per 2 caster levels
        /// </summary>
        public NearRange()
        {
            _Doubler = false;
        }

        /// <summary>
        /// 25 feet + 5 feet per 2 caster levels
        /// </summary>
        public NearRange(bool doubler)
        {
            _Doubler = doubler;
        }

        private bool _Doubler;
        public bool Doubler { get { return _Doubler; } }

        public override double EffectiveRange(CoreActor creature, int casterLevel)
        {
            // NOTE: creature may be null
            if (_Doubler)
                return 50d + 5d * casterLevel;
            else
                return 25d + 5d * Math.Floor(casterLevel / 2d);
        }

        public override RangeInfo ToRangeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToInfo<RangeInfo>(action, actor);
            return _info;
        }
    }
}
