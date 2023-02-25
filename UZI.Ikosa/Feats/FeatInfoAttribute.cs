using System;

namespace Uzi.Ikosa
{
    [AttributeUsage(AttributeTargets.Class), Serializable]
    public class FeatInfoAttribute: Attribute
    {
        public FeatInfoAttribute(string name, bool single)
        {
            Name = name;
            Singleton = single;
        }

        public FeatInfoAttribute(string name)
            : this(name, true)
        {
        }

        public string Name { get; private set; }
        public bool Singleton { get; private set; }
    }
}
