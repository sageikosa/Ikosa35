using System;
using Uzi.Core;

namespace Uzi.Ikosa
{
    [Serializable]
    public class SuccessCheck
    {
        #region Construction
        public SuccessCheck(ISupplyQualifyDelta checkQualify, int difficulty, object source)
            : this(checkQualify, difficulty, source, 0)
        {
        }

        public SuccessCheck(ISupplyQualifyDelta checkQualify, int difficulty, object source, int penaltyCost)
        {
            _Source = source;
            _Difficulty = difficulty;
            _CheckQualifier = new SoftQualifiedDelta(checkQualify);
            _PenaltyCost = penaltyCost;
        }
        #endregion

        #region Private Data
        private object _Source;
        private int _Difficulty;
        private int _PenaltyCost;
        private SoftQualifiedDelta _CheckQualifier;
        #endregion

        public object Source { get { return _Source; } }

        /// <summary>Negative number used as a voluntary penalty to gain some other benefit</summary>
        public int VoluntaryPenalty { get { return _PenaltyCost; } }

        /// <summary>Value to match or exceed to be successful</summary>
        public int Difficulty { get { return _Difficulty; } }

        /// <summary>Deltable that adds with the success check roll</summary>
        public SoftQualifiedDelta CheckQualified { get { return _CheckQualifier; } }
    }
}
