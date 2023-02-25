using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Items;
using Uzi.Packaging;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class ItemElement : ModuleElement, ICorePart
    {
        private IItemBase _Item;

        public ItemElement(IItemBase item, Description description)
            : base(description)
        {
            _Item = item;
        }

        public IItemBase ItemBase => _Item;

        public override Guid ID => _Item.ID;
        public string Name => Description.Message;
        public IEnumerable<ICorePart> Relationships => Enumerable.Empty<ICorePart>();
        public string TypeName => typeof(IItemBase).FullName;
    }
}
