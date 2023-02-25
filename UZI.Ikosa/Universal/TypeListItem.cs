using System;

namespace Uzi.Ikosa.Universal
{
    /// <summary>Used to present items that can be created</summary>
    public class TypeListItem
    {
        public TypeListItem(Type type, string description)
        {
            ListedType = type;
            Description = description;
        }
        public Type ListedType { get; private set; }
        public string Description { get; private set; }
    }
}
