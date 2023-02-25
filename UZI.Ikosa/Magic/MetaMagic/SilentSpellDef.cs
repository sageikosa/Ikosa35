using System;
using System.Collections.Generic;
using Uzi.Ikosa.Feats;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class SilentSpellDef : MetaMagicSpellDef
    {
        public SilentSpellDef(ISpellDef spellDef, Guid metaID, bool isSpontaneous)
            : base(spellDef, metaID, isSpontaneous)
        {
        }

        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                foreach (var _comp in Wrapped.ArcaneComponents)
                    if (!typeof(VerbalComponent).IsAssignableFrom(_comp.GetType()))
                        yield return _comp;
                yield break;
            }
        }

        public override IEnumerable<SpellComponent> DivineComponents
        {
            get
            {
                foreach (var _comp in Wrapped.DivineComponents)
                    if (!typeof(VerbalComponent).IsAssignableFrom(_comp.GetType()))
                        yield return _comp;
                yield break;
            }
        }

        public override string MetaTag => SilentSpellFeat.StaticMetaTag;
        public override int SlotAdjustment => 1;
        public override string Benefit => SilentSpellFeat.StaticMetaBenefit;
    }
}
