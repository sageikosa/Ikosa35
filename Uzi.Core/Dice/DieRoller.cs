using System;
using System.Linq;
using System.Security.Cryptography;
using Uzi.Core.Contracts;

namespace Uzi.Core.Dice
{
    /// <summary>DieRoller</summary>
    [Serializable]
    public class DieRoller : Roller
    {
        public DieRoller(byte sides)
            => Sides = sides;

        public byte Sides { get; private set; }

        private static RNGCryptoServiceProvider _Randomizer;
        internal static RNGCryptoServiceProvider GetRandomizer()
            => _Randomizer ??= new RNGCryptoServiceProvider();

        public override int RollValue(Guid? principalID, string title, string description, params Guid[] targets)
            => RollDie(principalID, Sides, title, description, targets);

        public override string ToString() => $@"1d{Sides}";

        public override RollerLog GetRollerLog()
            => new RollerLog { Expression = ToString(), Total = GenerateDie(Sides), Parts = null };

        public static int RollDie(Guid? principalID, byte sides, string title, string description, params Guid[] targets)
            => Roller.LogRollValue(title, description, principalID ?? Guid.Empty, (new DieRoller(sides)).GetRollerLog(), targets);

        #region protected static int GenerateDie(byte sides, RandomNumberGenerator generator)
        protected static int GenerateDie(byte sides)
        {
            var _randomizer = GetRandomizer();
            var _fence = (byte)((Byte.MaxValue / sides) * sides);
            var _bytes = new byte[] { 0 };
            do
            {
                _randomizer.GetBytes(_bytes);
            } while (_bytes[0] >= _fence);
            return (int)((_bytes[0] % sides) + 1);
        }
        #endregion

        public override int MaxRoll
            => Sides;

        public static DieRoller CreateRoller(StandardDieType standardDie)
            => new DieRoller((byte)standardDie);
    }
}
