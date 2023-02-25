using System;
using Uzi.Core;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Indicates that the power class must be suppressed</summary>
    [Serializable]
    public class PowerClassSuppress : Adjunct, IMonitorChange<Activation>
    {
        public PowerClassSuppress(object source, IPowerClass powerClass)
            : base(source)
        {
            _PowerClass = powerClass;
        }

        private IPowerClass _PowerClass;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            PowerClass.IsPowerClassActive = false;
            PowerClass.AddChangeMonitor(this);
        }

        protected override void OnDeactivate(object source)
        {
            PowerClass.RemoveChangeMonitor(this);
            PowerClass.IsPowerClassActive = true;
            base.OnDeactivate(source);
        }

        public IPowerClass PowerClass => _PowerClass;

        public override object Clone()
            => new PowerClassSuppress(Source, PowerClass);

        #region IMonitorChange<Activation> Members

        public void PreTestChange(object sender, AbortableChangeEventArgs<Activation> args)
        {
            if (args.NewValue.IsActive)
                args.DoAbort(@"Suppressed");
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<Activation> args) { }
        public void ValueChanged(object sender, ChangeValueEventArgs<Activation> args) { }

        #endregion
    }
}
