using System;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class DetectLaw : DetectAlignmentBase
    {
        protected override Alignment GetFilter() { return Alignment.LawfulNeutral; }
        public override string DisplayName { get { return @"Detect Law"; } }
        public override string Description { get { return @"Sense lawful presence, number, strength and direction."; } }
    }
}
