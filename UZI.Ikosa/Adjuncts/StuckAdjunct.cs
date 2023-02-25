using System;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// To unstick a door, must use a force open action.  This is almost identical to OpenBlock, except for the CanClose property.
    /// </summary>
    [Serializable]
    public class StuckAdjunct : Adjunct, IMonitorChange<OpenStatus>
    {
        public StuckAdjunct(object source)
            : base(source)
        {
        }

        protected override void OnActivate(object source)
        {
            IOpenable _openable = Anchor as IOpenable;
            if (_openable != null)
            {
                _openable.AddChangeMonitor(this);
            }
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            IOpenable _openable = Anchor as IOpenable;
            if (_openable != null)
            {
                _openable.RemoveChangeMonitor(this);
            }
            base.OnDeactivate(source);
        }

        #region IMonitorChange<OpenStatus> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<OpenStatus> args)
        {
            if (sender == Anchor)
            {
                if (args.OldValue.IsClosed)
                {
                    // stuck openable cannot move out of the closed position
                    args.DoAbort(@"Resisted");
                }
            }
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<OpenStatus> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<OpenStatus> args)
        {
        }
        #endregion

        public override object Clone()
        {
            return new StuckAdjunct(Source);
        }
    }
}