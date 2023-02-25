using System;
using System.Collections.Generic;
using Uzi.Ikosa.Feats;

namespace Uzi.Ikosa.Magic
{
    /// <summary>
    /// Excludes gesturing components from spell casting
    /// </summary>
    [Serializable]
    public class StillSpellDef : MetaMagicSpellDef
    {
        public StillSpellDef(ISpellDef spellDef, Guid metaID, bool isSpontaneous)
            : base(spellDef, metaID, isSpontaneous)
        {
        }

        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                foreach (var _comp in Wrapped.ArcaneComponents)
                    if (!typeof(SomaticComponent).IsAssignableFrom(_comp.GetType()))
                        yield return _comp;
                yield break;
            }
        }

        public override IEnumerable<SpellComponent> DivineComponents
        {
            get
            {
                foreach (var _comp in Wrapped.DivineComponents)
                    if (!typeof(SomaticComponent).IsAssignableFrom(_comp.GetType()))
                        yield return _comp;
                yield break;
            }
        }

        public override string MetaTag => StillSpellFeat.StaticMetaTag;
        public override int SlotAdjustment => 1;
        public override string Benefit => StillSpellFeat.StaticMetaBenefit;
    }
}
