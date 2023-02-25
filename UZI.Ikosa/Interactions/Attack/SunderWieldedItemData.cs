using Uzi.Core;

namespace Uzi.Ikosa.Interactions
{
    public class SunderWieldedItemData : InteractData
    {
        public SunderWieldedItemData(CoreActivity activity)
            : base(activity?.Actor)
        {
            _Activity = activity;
        }

        #region private data
        private CoreActivity _Activity;
        #endregion

        /// <summary>May be null if no activity was used during construction</summary>
        public CoreActor Attacker 
            => _Activity?.Actor;
    }
}
