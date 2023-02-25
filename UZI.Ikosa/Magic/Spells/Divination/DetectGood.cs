using System;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class DetectGood : DetectAlignmentBase
    {
        protected override Alignment GetFilter() { return Alignment.NeutralGood; }
        public override string DisplayName { get { return @"Detect Good"; } }
        public override string Description { get { return @"Sense good's presence, number, strength and direction."; } }
    }
}
