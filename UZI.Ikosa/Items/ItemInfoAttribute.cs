using System;
using Uzi.Visualize;

namespace Uzi.Ikosa.Items
{
    [AttributeUsage(AttributeTargets.Class), Serializable]
    public class ItemInfoAttribute: Attribute
    {
        public ItemInfoAttribute(string name, string description, string iconKey)
        {
            Name = name;
            Description = description;
            IconKey = iconKey;
        }

        public readonly string Name;
        public readonly string Description;
        public readonly string IconKey;
    }
}
