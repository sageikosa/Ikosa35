using System;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.TypeListers
{
    public class ItemTypeListItem
    {
        public ItemTypeListItem(Type itemType, ItemInfoAttribute info)
        {
            Info = info;
            ItemType = itemType;
        }

        public readonly ItemInfoAttribute Info;
        public readonly Type ItemType;
    }
}
