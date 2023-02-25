using System;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class DetectChaos : DetectAlignmentBase
    {
        protected override Alignment GetFilter() { return Alignment.ChaoticNeutral; }
        public override string DisplayName { get { return @"Detect Chaos"; } }
        public override string Description { get { return @"Sense chaotic presence, number, strength and direction."; } }
    }
}
