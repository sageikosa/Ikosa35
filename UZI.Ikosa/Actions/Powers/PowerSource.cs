using System;
using Uzi.Core;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Used as a source for interactions</summary>
    [Serializable]
    public abstract class PowerSource : InteractData, IPowerSource
    {
        protected PowerSource(IPowerClass powerClass, int powerLevel, IPowerDef powerDef)
            : base(null)
        {
            _Class = powerClass;
            _Def = powerDef.ForPowerSource();
            _PowerLevel = powerLevel;
        }

        #region private data
        private IPowerClass _Class;
        private IPowerDef _Def;
        private int _PowerLevel;
        #endregion

        public IPowerClass PowerClass => _Class;
        public Guid ID => PowerClass.OwnerID;
        public IPowerDef PowerDef => _Def;

        /// <summary>Level of the power when in use (may be heightened)</summary>
        public int PowerLevel => _PowerLevel;
    }
}
