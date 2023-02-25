using System;
using Uzi.Core;

namespace Uzi.Ikosa.Creatures
{
    /// <summary>Tracks amount of time without breathing</summary>
    [Serializable]
    public class BreathlessCounter : ICreatureBound
    {
        #region construction
        /// <summary>Tracks amount of time without breathing</summary>
        public BreathlessCounter(Creature critter)
        {
            _Critter = critter;
            _HeldCount = 0;
            _ContinueDifficulty = 10;
        }
        #endregion

        #region private data
        private Creature _Critter;
        private int _HeldCount;
        private int _ContinueDifficulty;
        #endregion

        #region public int HeldCount { get; set; }
        /// <summary>
        /// <para>Number of (virtual) rounds breath has been held.</para>
        /// <para>Strenuous activity may double up on increases.</para>
        /// </summary>
        public int HeldCount
        {
            get { return _HeldCount; }
            set
            {
                if (value >= 0)
                    _HeldCount = value;
                else
                    _HeldCount = 0;
            }
        }
        #endregion

        #region public int ContinueDifficulty { get; set; }
        /// <summary>
        /// <para>Effective difficulty to continue once checks must be made.</para>
        /// <para>May go down during recovery.</para>
        /// <para>Floors at 10</para>
        /// </summary>
        public int ContinueDifficulty
        {
            get { return _ContinueDifficulty; }
            set
            {
                // allow it to be adjusted over the floor value
                if (value >= 10)
                    _ContinueDifficulty = value;
                else
                    _ContinueDifficulty = 10;
            }
        }
        #endregion

        #region public bool MustCheck { get; }
        /// <summary>True if a constitution check must be made</summary>
        public bool MustCheck
        {
            get
            {
                // see if we've exceeded allowable time
                var _allowable = Creature.Abilities.Constitution.QualifiedValue(new Qualifier(Creature, this, null));
                if (_allowable < HeldCount)
                {
                    return true;
                }
                return false;
            }
        }
        #endregion

        #region ICreatureBound Members

        public Creature Creature { get { return _Critter; } }

        #endregion
    }
}
