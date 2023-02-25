using System;
using System.Security.Cryptography;
using Uzi.Core.Contracts;

namespace Uzi.Core.Dice
{

    /// <summary>Constant Values in Roller Expressions</summary>
    [Serializable]
    public class ConstantRoller : Roller
    {
        public ConstantRoller(int val)
            => _Value = val;

        protected int _Value;

        // unlogged

        public override int RollValue(Guid? principalID, string title, string description, params Guid[] targets)
            => _Value;

        // logged

        public override RollerLog GetRollerLog()
            => new RollerLog { Expression = ToString(), Total = _Value, Parts = null };

        public override string ToString() => _Value.ToString();

        public override int MaxRoll => _Value;
    };
}
