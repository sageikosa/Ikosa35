using System;
using System.Collections.Generic;
using Uzi.Ikosa.Feats;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class ExtendSpellDef : MetaMagicSpellDef
    {
        public ExtendSpellDef(ISpellDef spellDef, Guid metaID, bool isSpontaneous)
            : base(spellDef, metaID, isSpontaneous)
        {
        }

        public override IEnumerable<ISpellMode> SpellModes
        {
            get
            {
                foreach (var _mode in Wrapped.SpellModes)
                {
                    yield return new ExtendSpellMode(_mode);
                }
            }
        }

        public override string MetaTag => ExtendSpellFeat.StaticMetaTag;
        public override int SlotAdjustment => 1;
        public override string Benefit => ExtendSpellFeat.StaticMetaBenefit;
    }
}
