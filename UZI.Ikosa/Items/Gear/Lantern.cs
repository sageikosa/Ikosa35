using System;

namespace Uzi.Ikosa.Items.Gear
{
    [Serializable]
    public class Lantern : ItemBase
    {
        public Lantern()
            : base(@"Lantern", Size.Miniature)
        {
        }

        // TODO: use power battery to keep track of usable time (oil can replenish)

        protected override string ClassIconKey { get { return string.Empty; } }
    }
}
