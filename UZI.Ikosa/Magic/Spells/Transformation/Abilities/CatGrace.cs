using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Ikosa.Abilities;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class CatGrace : EnhanceAbility
    {
        #region components
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
        #endregion

        public override string DisplayName
            => @"Cat's Grace";

        protected override string AbilityMnemonic
            => MnemonicCode.Dex;
    }
}