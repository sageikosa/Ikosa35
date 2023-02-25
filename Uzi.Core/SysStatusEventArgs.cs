using System;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    public class SysStatusEventArgs : EventArgs
    {
        public SysStatusEventArgs(SysNotify sysStatus, Guid[] targets)
        {
            SysStatus = sysStatus;
            Targets = targets;
        }

        public SysNotify SysStatus { get; set; }
        public Guid[] Targets { get; set; }
    }
}
