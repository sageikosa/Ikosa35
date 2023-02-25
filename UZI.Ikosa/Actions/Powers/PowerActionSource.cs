using System;
using Uzi.Core;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Used as a source for interactions</summary>
    [Serializable]
    public abstract class PowerActionSource : PowerSource, IPowerActionSource
    {
        protected PowerActionSource(IPowerClass powerClass, int powerLevel, IPowerActionDef powerDef) :
            base(powerClass, powerLevel, powerDef)
        {
        }

        public IPowerActionDef PowerActionDef 
            => PowerDef as IPowerActionDef; 

        public IVolatileValue ActionClassLevel 
            => PowerClass.ClassPowerLevel; 

        public abstract void UsePower();
        public abstract string DisplayName { get; }
    }
}
