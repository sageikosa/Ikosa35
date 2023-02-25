using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Ikosa.Abilities;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class BearEndurance : EnhanceAbility
    {
        #region arcane/divine components
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
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
            => @"Bear's Endurance";

        protected override string AbilityMnemonic
            => MnemonicCode.Con;
    }
}
