using System;

namespace Uzi.Ikosa.Creatures.SubTypes
{
    [Serializable]
    public class FireSubType : CreatureElementalSubType
    {
        public FireSubType(object source) : base(source) { }
        public override string Name { get { return @"Fire"; } }

        public override CreatureSubType Clone(object source)
        {
            return new FireSubType(source);
        }
    }
}
