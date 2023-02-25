using Uzi.Core;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// Represents sound in environmental transit
    /// </summary>
    public class SoundTransit : InteractData
    {
        #region construction
        public SoundTransit(IAudible audible)
            : base(null)
        {
            _Audible = audible;
            _Difficulty = new Deltable(0);
        }
        #endregion

        #region private data
        private IAudible _Audible;
        private Deltable _Difficulty;
        #endregion

        public Deltable AddedDifficulty => _Difficulty;
        public IAudible Audible => _Audible;
    }
}
