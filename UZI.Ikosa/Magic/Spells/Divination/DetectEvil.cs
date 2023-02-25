using System;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class DetectEvil : DetectAlignmentBase
    {
        protected override Alignment GetFilter() { return Alignment.NeutralEvil; }
        public override string DisplayName { get { return @"Detect Evil"; } }
        public override string Description { get { return @"Sense evil's presence, number, strength and direction."; } }
    }
}
