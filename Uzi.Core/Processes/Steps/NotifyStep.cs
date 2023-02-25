using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    [Serializable]
    public class NotifyStep : CoreStep
    {
        #region Construction
        public NotifyStep(CoreProcess process, SysNotify notify) :
            base(process)
        {
            _Notify = notify;
        }
        #endregion

        #region data
        private readonly SysNotify _Notify;
        #endregion

        protected override bool OnDoStep()
        {
            if (_Notify != null)
                Process.ProcessManager.SendSysStatus(_Notify, InfoReceivers);
            return true;
        }

        public Guid[] InfoReceivers { get; set; }

        protected override StepPrerequisite OnNextPrerequisite() => null;
        public override bool IsDispensingPrerequisites => false;
        public override string Name => @"Notify";
        public SysNotify SysNotify => _Notify;
    }
}
