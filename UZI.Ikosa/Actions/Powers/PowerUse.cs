using System;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Represents the action of using a power [Action]</summary>
    [Serializable]
    public abstract class PowerUse<PowerSrc> : ActionBase, IPowerUse<PowerSrc>
        where PowerSrc : class, IPowerActionSource
    {
        #region construction
        /// <summary>Represents the action of using a power [Action]</summary>
        protected PowerUse(PowerSrc powerSource, string orderKey)
            : base(powerSource, new ActionTime(TimeType.Regular), false, false, orderKey)
        {
        }

        /// <summary>Represents the action of using a power [Action]</summary>
        protected PowerUse(PowerSrc powerSource, ActionTime actionTime, string orderKey)
            : base(powerSource, actionTime, false, false, orderKey)
        {
        }

        protected PowerUse(PowerSrc powerSource, ActionTime actionTime, bool provokesMelee, bool provokesTarget, string orderKey)
            : base(powerSource, actionTime, provokesMelee, provokesTarget, orderKey)
        {
        }
        #endregion

        /// <summary>Action.Source as PowerSrc</summary>
        public PowerSrc PowerActionSource => Source as PowerSrc;

        /// <summary>Action.Source as IPowerActionSource</summary>
        public IPowerActionSource IPowerActionSource => PowerActionSource;

        public override string Key => PowerActionSource.PowerActionDef.Key;
        public override string DisplayName(CoreActor actor) => PowerActionSource.PowerActionDef.DisplayName;
        public abstract ICapabilityRoot CapabilityRoot { get; }
        public abstract void ActivatePower(PowerActivationStep<PowerSrc> step);
        public abstract void ApplyPower(PowerApplyStep<PowerSrc> step);
        public abstract PowerAffectTracker PowerTracker { get; }

        public override ActionInfo ToActionInfo(IActionProvider provider, bool isExternal, CoreActor actor)
        {
            var _info = ToInfo<PowerActionInfo>(provider, isExternal, actor);
            _info.PowerDef = PowerActionSource.PowerDef.ToPowerDefInfo();
            _info.PowerLevel = PowerActionSource.PowerLevel;
            return _info;
        }
    }
}
