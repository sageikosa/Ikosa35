using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Abilities;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class OwlWisdom : EnhanceAbility
    {
        #region arcane/divine components
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new MaterialComponent();
                yield break;
            }
        }

        public override IEnumerable<SpellComponent> DivineComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new DivineFocusComponent();
                yield break;
            }
        }
        #endregion

        public override string DisplayName
            => @"Owl's Wisdom";

        protected override string AbilityMnemonic
            => MnemonicCode.Wis;
    }
}
