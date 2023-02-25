using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Items
{
    /// <summary>
    /// Provides access to actions from adjuncts on a slotted item
    /// </summary>
    [Serializable]
    public class SlottedItemActionProvider : SlotActivation
    {
        /// <summary>Provides access to actions from adjuncts on a slotted item</summary>
        public SlottedItemActionProvider(IActionProvider provider)
            : base(provider, true)
        {
        }

        public override bool IsProtected => true;
        public IActionProvider Provider => Source as IActionProvider;

        public override object Clone()
            => new SlottedItemActionProvider(Provider);

        public override IEnumerable<Info> IdentificationInfos { get { yield break; } }

        protected override void OnSlottedActivate()
            => SlottedItem?.CreaturePossessor?.Actions.Providers.Add(this, Provider);

        protected override void OnSlottedDeActivate()
            => SlottedItem?.CreaturePossessor?.Actions.Providers.Remove(this);
    }
}
