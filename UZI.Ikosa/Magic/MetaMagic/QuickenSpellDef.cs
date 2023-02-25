using System;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Feats;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class QuickenSpellDef : MetaMagicSpellDef
    {
        public QuickenSpellDef(ISpellDef spellDef, Guid metaID, bool isSpontaneous)
            : base(spellDef, metaID, isSpontaneous)
        {
        }

        public override ActionTime ActionTime => new ActionTime(TimeType.Twitch);

        public override string MetaTag => QuickenSpellFeat.StaticMetaTag;
        public override int SlotAdjustment => 4;
        public override string Benefit => QuickenSpellFeat.StaticMetaBenefit;
    }
}
