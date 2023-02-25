using System;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    /// <summary>
    /// Used mainly to pass target information internally when calculated.
    /// </summary>
    /// <typeparam name="ValType"></typeparam>
    [Serializable]
    public class ValueTarget<ValType> : AimTarget
    {
        public ValueTarget(string key, ValType val)
            : base(key, null)
        {
            _Val = val;
        }

        private ValType _Val;
        public ValType Value { get { return _Val; } set { _Val = value; } }

        public override AimTargetInfo GetTargetInfo()
        {
            if (typeof(ValType) == typeof(int?))
            {
                var _this = this as ValueTarget<int?>;
                return new ValueIntTargetInfo
                {
                    Key = Key,
                    TargetID = Target != null ? Target.ID : (Guid?)null,
                    Value = _this.Value
                };
            }
            else if (typeof(ValType) == typeof(int))
            {
                var _this = this as ValueTarget<int>;
                return new ValueIntTargetInfo
                {
                    Key = Key,
                    TargetID = Target != null ? Target.ID : (Guid?)null,
                    Value = _this.Value
                };
            }
            return null;
        }

        public static ValueTarget<int?> GetTargetInt(AimTargetInfo info)
        {
            if (info is ValueIntTargetInfo _val)
            {
                return new ValueTarget<int?>(info.Key, _val.Value);
            }
            return null;
        }
    }
}
