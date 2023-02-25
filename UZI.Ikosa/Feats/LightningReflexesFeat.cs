using System;
using Uzi.Core;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Lightning Reflexes")
    ]
    public class LightningReflexesFeat: FeatBase
    {
        public LightningReflexesFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get { return "+2 Reflex Saves"; }
        }

        protected Delta pModifier;

        protected override void OnAdd()
        {
            // add to feat list
            base.OnAdd();

            // modify saving throw
            pModifier = new Delta(2, this.GetType(), this.Name);
            _Creature.ReflexSave.Deltas.Add(pModifier);
        }

        protected override void OnRemove()
        {
            // remove from feat list
            base.OnRemove();

            // unmodify saving throw
            pModifier.DoTerminate();
        }
    }
}
