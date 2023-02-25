using System;

namespace Uzi.Core
{
    public class AbortableEventArgs : EventArgs
    {
        #region Construction
        public AbortableEventArgs()
        {
            _Abort = true;
            Reason = string.Empty;
            Aborter = null;
            _Action = string.Empty;
        }

        public AbortableEventArgs(string action)
        {
            _Abort = true;
            Reason = string.Empty;
            Aborter = null;
            _Action = action;
        }
        #endregion

        #region PRivate Data
        private string _Action;
        protected bool _Abort;
        private string _Reason;
        private object _Aborter;
        #endregion

        /// <summary>Action that caused the event</summary>
        public string Action { get { return _Action; } }

        /// <summary>Indication whether the change was aborted</summary>
        public bool Abort { get { return _Abort; } }

        /// <summary>(Optional) Reason the aborter aborted the change</summary>
        public string Reason { get { return _Reason; } set { _Reason = value; } }

        /// <summary>Monitor that aborted the change</summary>
        public object Aborter { get { return _Aborter; } set { _Aborter = value; } }

        public void DoAbort()
        {
            _Abort = true;
        }

        public void DoAbort(string reason)
        {
            DoAbort();
            Reason = reason;
        }

        public void DoAbort(string reason, object aborter)
        {
            DoAbort(reason);
            Aborter = aborter;
        }
    }
}
