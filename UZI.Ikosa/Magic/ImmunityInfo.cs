using System;

namespace Uzi.Ikosa.Magic
{
    /// <summary>
    /// Information on immunity and end-time (used by ImmunityTracker)
    /// </summary>
    [Serializable]
    public struct ImmunityInfo
    {
        #region private data
        private bool _Immune;
        private double _ImmunityEndTime;
        #endregion

        public bool Immune { get { return _Immune; } set { _Immune = value; } }
        public double ImmunityEndTime { get { return _ImmunityEndTime; } set { _ImmunityEndTime = value; } }
    }
}
