using System;
using Uzi.Core;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FighterBonusFeat,
    FeatInfo("Improved Initiative")
    ]
    public class ImprovedInitiativeFeat : FeatBase
    {
        public ImprovedInitiativeFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get { return @"+4 Initiative Checks"; }
        }

        protected Delta _Modifier;

        protected override void OnAdd()
        {
            // add to feat list
            base.OnAdd();

            // modify saving throw
            _Modifier = new Delta(4, GetType(), Name);
            _Creature.Initiative.Deltas.Add(_Modifier);
        }

        protected override void OnRemove()
        {
            // remove from feat list
            base.OnRemove();

            // unmodify saving throw
            _Modifier.DoTerminate();
        }
    }
}