using System;
using System.Text;

namespace Uzi.Ikosa
{
    public enum LawChaosAxis
    {
        Lawful = 1, Neutral = 0, Chaotic = -1
    }

    public enum GoodEvilAxis
    {
        Good = 10, Neutral = 0, Evil = -10
    }

    /// <summary>Good/Neural/Evil by Lawful/Neutral/Chaotic</summary>
    [Serializable]
    public sealed class Alignment
    {
        #region Constructor
        internal Alignment(LawChaosAxis lawChaos, GoodEvilAxis goodEvil)
        {
            _Orderliness = lawChaos;
            _Ethical = goodEvil;
        }
        #endregion

        /// <summary>Determines whether the alignment is detected by a specific axial alignment</summary>
        public bool DetectsAs(Alignment axialAlignment)
        {
            if (axialAlignment.IsAxial)
            {
                if (axialAlignment.Ethicality != GoodEvilAxis.Neutral)
                    return axialAlignment.Ethicality == Ethicality;
                return axialAlignment.Orderliness == Orderliness;
            }
            else
                return false;
        }

        /// <summary>One (and only one) of the alignment axes must be neutral</summary>
        public bool IsAxial { get { return (Orderliness == LawChaosAxis.Neutral) ^ (Ethicality == GoodEvilAxis.Neutral); } }

        /// <summary>Alignment is true neutral (not really aligned nor opposed)</summary>
        public bool IsNeutral { get { return GetHashCode() == 0; } }

        #region public LawChaosAxis Orderliness {get;}
        private LawChaosAxis _Orderliness;
        public LawChaosAxis Orderliness
        {
            get
            {
                return _Orderliness;
            }
        }
        #endregion

        #region public GoodEvilAxis Ethicality {get;}
        private GoodEvilAxis _Ethical;
        public GoodEvilAxis Ethicality
        {
            get
            {
                return _Ethical;
            }
        }
        #endregion

        #region public string ItemTagString()
        /// <summary>Formal adjectives for items and creatures</summary>
        public string ItemTagString()
        {
            var _affinity = new StringBuilder();
            if (_Orderliness == LawChaosAxis.Lawful)
                _affinity.Append(@"Axiomatic");
            else if (_Orderliness == LawChaosAxis.Chaotic)
                _affinity.Append(@"Anarchic");

            if (_Ethical == GoodEvilAxis.Good)
                _affinity.Append(@" Holy");
            else if (_Ethical == GoodEvilAxis.Evil)
                _affinity.Append(@" Unholy");

            return _affinity.ToString().TrimStart(null);
        }
        #endregion

        #region public string NoNeutralString()
        /// <summary>ToString without neutrals, for describing DR/alignment, etc.</summary>
        public string NoNeutralString()
        {
            var _neutral = new StringBuilder();
            if (_Orderliness != LawChaosAxis.Neutral)
                _neutral.Append(_Orderliness.ToString());

            if (_Ethical != GoodEvilAxis.Neutral)
                _neutral.AppendFormat(@" {0}", _Ethical.ToString());

            return _neutral.ToString().TrimStart(null);
        }
        #endregion

        #region public override string ToString()
        public override string ToString()
        {
            if ((_Ethical == GoodEvilAxis.Neutral))
            {
                // conventionally, true neutral
                if (_Orderliness == LawChaosAxis.Neutral)
                    return @"true neutral";
            }
            return $@"{_Orderliness } {_Ethical }";
        }
        #endregion

        /// <summary>both true neutral, or matching on non-neutral axis</summary>
        public bool IsMatchingAxial(Alignment comparitor)
        {
            if (IsNeutral)
                return comparitor.IsNeutral;
            return ((Ethicality == GoodEvilAxis.Neutral) || (Ethicality == comparitor.Ethicality))
                && ((Orderliness == LawChaosAxis.Neutral) || (Orderliness == comparitor.Orderliness));
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var _align = obj as Alignment;
            return _align.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode() { return (int)Orderliness + (int)Ethicality; }

        /// <summary>True if the comparitor is opposable to "this" alignment.</summary>
        public bool Opposable(Alignment comparitor)
        {
            bool _opposable;
            if (Ethicality == GoodEvilAxis.Neutral)
                _opposable = true;
            else
                _opposable = (Ethicality != comparitor.Ethicality);

            if (Orderliness == LawChaosAxis.Neutral)
                _opposable &= true;
            else
                _opposable &= (Orderliness != comparitor.Orderliness);

            return _opposable;
        }

        #region LawfulGood
        private static Alignment psLawfulGood = new Alignment(LawChaosAxis.Lawful, GoodEvilAxis.Good);
        public static Alignment LawfulGood
        {
            get
            {
                return psLawfulGood;
            }
        }
        #endregion

        #region NeutralGood
        private static Alignment psNeutralGood = new Alignment(LawChaosAxis.Neutral, GoodEvilAxis.Good);
        public static Alignment NeutralGood
        {
            get
            {
                return psNeutralGood;
            }
        }
        #endregion

        #region ChaoticGood
        private static Alignment psChaoticGood = new Alignment(LawChaosAxis.Chaotic, GoodEvilAxis.Good);
        public static Alignment ChaoticGood
        {
            get
            {
                return psChaoticGood;
            }
        }
        #endregion

        #region LawfulNeutral
        private static Alignment psLawfulNeutral = new Alignment(LawChaosAxis.Lawful, GoodEvilAxis.Neutral);
        public static Alignment LawfulNeutral
        {
            get
            {
                return psLawfulNeutral;
            }
        }
        #endregion

        #region TrueNeutral
        private static Alignment psTrueNeutral = new Alignment(LawChaosAxis.Neutral, GoodEvilAxis.Neutral);
        public static Alignment TrueNeutral
        {
            get
            {
                return psTrueNeutral;
            }
        }
        #endregion

        #region ChaoticNeutral
        private static Alignment psChaoticStupid = new Alignment(LawChaosAxis.Chaotic, GoodEvilAxis.Neutral);
        public static Alignment ChaoticNeutral
        {
            get
            {
                return psChaoticStupid;
            }
        }
        #endregion

        #region LawfulEvil
        private static Alignment psLawfulEvil = new Alignment(LawChaosAxis.Lawful, GoodEvilAxis.Evil);
        public static Alignment LawfulEvil
        {
            get
            {
                return psLawfulEvil;
            }
        }
        #endregion

        #region NeutralEvil
        private static Alignment psNeutralEvil = new Alignment(LawChaosAxis.Neutral, GoodEvilAxis.Evil);
        public static Alignment NeutralEvil
        {
            get
            {
                return psNeutralEvil;
            }
        }
        #endregion

        #region ChaoticEvil
        private static Alignment psChaoticEvil = new Alignment(LawChaosAxis.Chaotic, GoodEvilAxis.Evil);
        public static Alignment ChaoticEvil
        {
            get
            {
                return psChaoticEvil;
            }
        }
        #endregion

    };
}
