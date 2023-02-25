using System;
using Uzi.Core;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo(@"Great Fortitude")
    ]
    public class GreatFortitudeFeat: FeatBase
    {
        public GreatFortitudeFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get { return @"+2 Fortitude Saves"; }
        }

        protected Delta _Modifier;

        #region OnAdd and OnRemove
        protected override void OnAdd()
        {
            // add to feat list
            base.OnAdd();

            // modify saving throw
            _Modifier = new Delta(2, this.GetType(), this.Name);
            _Creature.FortitudeSave.Deltas.Add(_Modifier);
        }

        protected override void OnRemove()
        {
            // remove from feat list
            base.OnRemove();

            // unmodify saving throw
            _Modifier.DoTerminate();
        }
        #endregion
    }
}
