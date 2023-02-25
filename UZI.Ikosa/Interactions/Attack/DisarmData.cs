using Uzi.Core;

namespace Uzi.Ikosa.Interactions
{
    public class DisarmData : InteractData
    {
        public DisarmData(CoreActivity activity)
            : base(activity != null ? activity.Actor : null)
        {
            _Activity = activity;
        }

        #region private data
        private CoreActivity _Activity;
        #endregion

        /// <summary>May be null if no activity was used during construction</summary>
        public CoreActor Attacker { get { return _Activity != null ? _Activity.Actor : null; } }
    }
}
