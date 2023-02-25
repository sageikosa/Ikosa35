using System;

namespace Uzi.Core
{
    public class StepEventArgs : EventArgs
    {
        public StepEventArgs(CoreStep step)
            : base()
        {
            this.Step = step;
        }

        public CoreStep Step { get; set; }
    }
}
