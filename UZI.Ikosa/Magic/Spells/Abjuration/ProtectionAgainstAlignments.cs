using System;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class ProtectionAgainstEvil : ProtectAgainstAlignmentBase
    {
        public override string DisplayName { get { return @"Protection against Evil"; } }
        public override string Description { get { return @"+2 Armor and Save Bonuses against evil creatures"; } }
        protected override Alignment WardedAlignment() { return Alignment.NeutralEvil; }
    }

    [Serializable]
    public class ProtectionAgainstGood : ProtectAgainstAlignmentBase
    {
        public override string DisplayName { get { return @"Protection against Good"; } }
        public override string Description { get { return @"+2 Armor and Save Bonuses against good creatures"; } }
        protected override Alignment WardedAlignment() { return Alignment.NeutralGood; }
    }

    [Serializable]
    public class ProtectionAgainstLaw : ProtectAgainstAlignmentBase
    {
        public override string DisplayName { get { return @"Protection against Law"; } }
        public override string Description { get { return @"+2 Armor and Save Bonuses against lawful creatures"; } }
        protected override Alignment WardedAlignment() { return Alignment.LawfulNeutral; }
    }

    [Serializable]
    public class ProtectionAgainstChaos : ProtectAgainstAlignmentBase
    {
        public override string DisplayName { get { return @"Protection against Chaos"; } }
        public override string Description { get { return @"+2 Armor and Save Bonuses against chaotic creatures"; } }
        protected override Alignment WardedAlignment() { return Alignment.ChaoticNeutral; }
    }
}
