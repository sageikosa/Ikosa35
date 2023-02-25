using System;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// When added to an IOpenable, prevents it from moving out of (or into) the closed position.
    /// </summary>
    [Serializable]
    public class OpenBlocked : Adjunct, IMonitorChange<OpenStatus>
    {
        public OpenBlocked(object source, IForceOpenTarget forceOpenTarget, bool allowClose)
            : base(source)
        {
            _CanClose = allowClose;
            _ForceOpen = forceOpenTarget;
        }

        #region data
        private bool _CanClose;
        private IForceOpenTarget _ForceOpen;
        #endregion

        public bool CanClose { get => _CanClose; set => _CanClose = value; }

        public IForceOpenTarget ForcedOpenTarget => _ForceOpen;

        protected override void OnActivate(object source)
        {
            (Anchor as IOpenable)?.AddChangeMonitor(this);
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            (Anchor as IOpenable)?.RemoveChangeMonitor(this);
            base.OnDeactivate(source);
        }

        #region IMonitorChange<OpenStatus> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<OpenStatus> args)
        {
            if (sender == Anchor)
            {
                if ((args.NewValue.IsClosed && !CanClose) || args.OldValue.IsClosed)
                {
                    // locked openable cannot move out of the closed position
                    // locked openable cannot move into the closed position unless it is designed to allow it
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
            => new OpenBlocked(Source, ForcedOpenTarget, CanClose);
    }

    public interface IForceOpenTarget : ICore
    {
        void DoForcedOpen();
    }
}
