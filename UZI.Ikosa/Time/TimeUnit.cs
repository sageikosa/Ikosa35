using System;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public abstract class TimeUnit
    {
        public abstract double BaseUnitFactor { get; }
        public abstract string UnitName { get; }
        public abstract string PluralName { get; }
        public string ValueName(int value)
            => value != 1 ? PluralName : UnitName;
        public string ValueName(double value)
            => value != 1d ? PluralName : UnitName;
    }

    [Serializable]
    public class Round : TimeUnit
    {
        public override double BaseUnitFactor => UnitFactor;
        /// <summary>1 round</summary>
        public static double UnitFactor => 1;
        public override string UnitName => nameof(Round);
        public override string PluralName => $@"{UnitName}s";
    }

    [Serializable]
    public class Minute : TimeUnit
    {
        public override double BaseUnitFactor => UnitFactor;
        /// <summary>10 rounds</summary>
        public static double UnitFactor => 10;
        public override string UnitName => nameof(Minute);
        public override string PluralName => $@"{UnitName}s";
    }

    [Serializable]
    public class Hour : TimeUnit
    {
        public override double BaseUnitFactor => UnitFactor;
        /// <summary>600 rounds</summary>
        public static double UnitFactor => 600;
        public override string UnitName => nameof(Hour);
        public override string PluralName => $@"{UnitName}s";
    }

    [Serializable]
    public class Day : TimeUnit
    {
        public override double BaseUnitFactor => UnitFactor;
        /// <summary>14400 rounds</summary>
        public static double UnitFactor => 14400;
        public override string UnitName => nameof(Day);
        public override string PluralName => $@"{UnitName}s";

        /// <summary>Gets time marking start of current day</summary>
        public static double StartOfDay(double time)
            => Math.Floor(time / Day.UnitFactor) * Day.UnitFactor;

        /// <summary>Gets time marking end of current day (or start of next day)</summary>
        public static double EndOfDay(double time)
            => (Math.Floor(time / Day.UnitFactor) + 1d) * Day.UnitFactor;
    }

    /// <summary>Week is 7 days</summary>
    [Serializable]
    public class Week : TimeUnit
    {
        public override double BaseUnitFactor => UnitFactor;
        /// <summary>100800 rounds</summary>
        public static double UnitFactor => 100800;
        public override string UnitName => nameof(Week);
        public override string PluralName => $@"{UnitName}s";
    }

    /// <summary>Month is 30 days</summary>
    [Serializable]
    public class Month : TimeUnit
    {
        public override double BaseUnitFactor => UnitFactor;
        /// <summary>432000 rounds</summary>
        public static double UnitFactor => 432000;
        public override string UnitName => nameof(Month);
        public override string PluralName => $@"{UnitName}s";
    }

    /// <summary>Year is 365 days</summary>
    [Serializable]
    public class Year : TimeUnit
    {
        public override double BaseUnitFactor => UnitFactor;
        /// <summary>5256000 rounds</summary>
        public static double UnitFactor => 5256000;
        public override string UnitName => nameof(Year);
        public override string PluralName => $@"{UnitName}s";
    }
}
