using System;
using Uzi.Core;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Iron Will")
    ]
    public class IronWillFeat: FeatBase 
    {
        public IronWillFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get { return "+2 Will Saves"; }
        }

        protected Delta _Modifier;

        protected override void OnAdd()
        {
            // add to feat list
            base.OnAdd();

            // modify saving throw
            _Modifier = new Delta(2, this.GetType(), this.Name);
            _Creature.WillSave.Deltas.Add(_Modifier);
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
