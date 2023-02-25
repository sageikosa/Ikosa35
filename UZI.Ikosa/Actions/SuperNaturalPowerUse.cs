using System;
using Uzi.Core;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Represents the action of using a super natural ability [Action]</summary>
    [Serializable]
    public abstract class SuperNaturalPowerUse : PowerUse<SuperNaturalPowerActionSource>
    {
        /// <summary>Represents the action of using a super natural ability [Action]</summary>
        protected SuperNaturalPowerUse(SuperNaturalPowerActionSource source, string orderKey)
            : base(source, orderKey)
        {
        }

        /// <summary>Represents the action of using a super natural ability [Action]</summary>
        protected SuperNaturalPowerUse(SuperNaturalPowerActionSource source, ActionTime actionTime, string orderKey)
            : base(source, actionTime, orderKey)
        {
        }

        public override ICapabilityRoot CapabilityRoot 
            => PowerActionSource.SuperNaturalPowerActionDef;

        /// <summary>Standard class power level when using this power</summary>
        public abstract int StandardClassPowerLevel { get; }
    }
}
