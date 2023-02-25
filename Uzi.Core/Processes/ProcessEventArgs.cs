using System;

namespace Uzi.Core
{
    public class ProcessEventArgs : EventArgs
    {
        public ProcessEventArgs(CoreProcess process)
            : base()
        {
            this.Process = process;
        }

        public CoreProcess Process { get; set; }
    }
}
