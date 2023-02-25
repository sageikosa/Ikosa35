using System;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class MaterialGripRule : GripRule
    {
        #region private data
        private GripDisposition _Disposition;
        private bool _IsPlus;
        #endregion

        public GripDisposition Disposition { get { return _Disposition; } set { _Disposition = value; } }
        public bool IsPlusMaterial { get { return _IsPlus; } set { _IsPlus = value; } }
    }

    [Serializable]
    public enum GripDisposition
    {
        /// <summary>This material extends over an entire external face</summary>
        Full,
        /// <summary>An internal surface consists of this material</summary>
        Internal,
        /// <summary>This material makes up part of an external face</summary>
        Rectangular,
        /// <summary>This material makes up part of an external face in a complex geometry</summary>
        Irregular
    }
}
