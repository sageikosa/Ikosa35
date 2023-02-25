using System;
using System.Collections.Generic;
using Uzi.Ikosa.Feats;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class MaximizeSpellDef : MetaMagicSpellDef
    {
        public MaximizeSpellDef(ISpellDef spellDef, Guid metaID, bool isSpontaneous)
            : base(spellDef, metaID, isSpontaneous)
        {
        }

        public override IEnumerable<ISpellMode> SpellModes
        {
            get
            {
                foreach (var _mode in Wrapped.SpellModes)
                {
                    yield return new MaximizeSpellMode(_mode);
                }
            }
        }

        public override string MetaTag => MaximizeSpellFeat.StaticMetaTag;
        public override int SlotAdjustment => 3;
        public override string Benefit => MaximizeSpellFeat.StaticMetaBenefit;
    }
}
