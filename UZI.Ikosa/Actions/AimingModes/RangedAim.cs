using System;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// [AimingMode]
    /// </summary>
    [Serializable]
    public abstract class RangedAim : AimingMode
    {
        #region Construction
        protected RangedAim(string key, string displayName, Range minModes, Range maxModes, Range range)
            : base(key, displayName, minModes, maxModes)
        {
            Range = range;
        }
        protected RangedAim(string key, string displayName, Range minModes, Range maxModes, Range range, string preposition)
            : base(key, displayName, minModes, maxModes, preposition)
        {
            Range = range;
        }
        #endregion

        private Range _Range;
        public Range Range { get { return _Range; } set { _Range = value; } }

        protected RAInfo ToRangedAimInfo<RAInfo>(CoreAction action, CoreActor actor)
            where RAInfo: RangedAimInfo, new()
        {
            var _info = ToInfo<RAInfo>(action, actor);
            _info.Range = Range.EffectiveRange(actor, _info.ClassLevel);
            _info.RangeInfo = Range.ToRangeInfo(action, actor);
            return _info;
        }
    }
}
