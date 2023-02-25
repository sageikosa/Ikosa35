using System;
using System.Collections.Generic;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    /// <summary>Used (reactively) to terminate a process</summary>
    [Serializable]
    public class TerminationStep : NotifyStep
    {
        /// <summary>Used (reactively) to terminate a process</summary>
        public TerminationStep(CoreProcess process, SysNotify notify)
            : base(process, notify)
        {
        }

        public override string Name => @"Ending process"; 

        protected override bool OnDoStep()
        {
            Process.IsActive = false;
            return true;
        }
    }
}
