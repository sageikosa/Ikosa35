using System;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public abstract class ActorStateBase : Adjunct
    {
        protected ActorStateBase(object source)
            : base(source)
        {
        }

        protected Creature Critter
            => Anchor as Creature;

        protected double CurrentTime
            => Critter?.GetCurrentTime() ?? double.MaxValue;

        #region protected void StartProcess(CoreProcess process)
        protected void StartProcess(CoreProcess process)
        {
            var _located = Anchor.GetLocated();
            if (_located != null)
            {
                var _procMan = _located.Locator.MapContext.ContextSet.ProcessManager;
                _procMan.StartProcess(process);
            }
        }
        #endregion

        protected void NotifyStateChange(bool ending = false, bool awareness = false, bool sensors = false)
        {
            var _critter = Critter;
            if (_critter != null)
            {
                var _title = ending
                    ? $@"{_critter.Name} is no longer {this.SourceName()}"
                    : $@"{_critter.Name} is now {this.SourceName()}";
                _critter.SendSysNotify(new RefreshNotify(true, sensors, awareness, false, false));
            }
        }
    }
}
