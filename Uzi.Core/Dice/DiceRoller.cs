using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Uzi.Core.Contracts;

namespace Uzi.Core.Dice
{
    /// <summary>Dice Roller (for expressions like 3d6)</summary>
    [Serializable]
    public class DiceRoller : DieRoller
    {
        #region Constructor(int number, byte side)
        public DiceRoller(int number, byte sides)
            : base(sides)
        {
            Number = number;
            Factor = 1.0m;
        }

        public DiceRoller(int number, byte sides, decimal factor)
            : base(sides)
        {
            Number = number;
            Factor = factor;
        }
        #endregion

        public int Number { get; private set; }
        public decimal Factor { get; private set; }

        public override int RollValue(Guid? principalID, string title, string description, params Guid[] targets)
            => Convert.ToInt32(Convert.ToDecimal(RollDice(principalID, Number, Sides, title, description, targets)) * Factor);

        #region public override RollerLog GetRollerLog()
        public override RollerLog GetRollerLog()
        {
            var _list = new List<RollerLog>();
            for (var _rx = 0; _rx < Number; _rx++)
            {
                _list.Add(new RollerLog
                {
                    Expression = string.Format(@"d{0}", Sides),
                    Total = DieRoller.GenerateDie(Sides),
                    Parts = null
                });
            }

            // return
            return new RollerLog
            {
                Expression = ToString(),
                Total = Convert.ToInt32(Convert.ToDecimal(_list.Sum(_p => _p.Total)) * Factor),
                Parts = _list
            };
        }
        #endregion

        public static int RollDice(Guid? principalID, int number, byte sides, string title, string description, params Guid[] targets)
            => Roller.LogRollValue(title, description, principalID ?? Guid.Empty, (new DiceRoller(number, sides)).GetRollerLog(), targets);

        #region protected static int GenerateDice(int number, byte sides)
        protected static int GenerateDice(int number, byte sides)
        {
            var _result = 0;
            for (var lRolls = 0; lRolls < number; lRolls++)
            {
                _result += DieRoller.GenerateDie(sides);
            }
            return _result;
        }
        #endregion

        public override string ToString()
            => (Factor != 1.0m)
            ? $@"({Number}d{Sides})*{Factor}"
            : $@"{Number}d{Sides}";

        public override int MaxRoll
            => (int)(Sides * Number * Factor);
    }
}
