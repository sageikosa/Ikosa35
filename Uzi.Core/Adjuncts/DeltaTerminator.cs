using System;

namespace Uzi.Core
{
    /// <summary>On first deactivation, terminates the delta</summary>
    [Serializable]
    public class DeltaTerminator : Adjunct
    {
        public DeltaTerminator(Delta delta)
            : base(delta)
        {
        }

        protected override void OnDeactivate(object source)
        {
            Delta.DoTerminate();
            base.OnDeactivate(source);
        }

        public Delta Delta => Source as Delta;

        public override object Clone()
            => new DeltaTerminator(Delta);
    }
}
