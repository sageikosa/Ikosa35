using System;
using Uzi.Core;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class Squeezing : Adjunct
    {
        public Squeezing()
            : base(typeof(ITacticalMap))
        {
            _Squeeze = new Delta(-4, typeof(Squeezing));
        }

        private Delta _Squeeze;

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                _critter.BaseAttack.Deltas.Add(_Squeeze);
                _critter.NormalArmorRating.Deltas.Add(_Squeeze);
                _critter.TouchArmorRating.Deltas.Add(_Squeeze);
                _critter.IncorporealArmorRating.Deltas.Add(_Squeeze);
            }
            base.OnActivate(source);
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            _Squeeze.DoTerminate();
            base.OnDeactivate(source);
        }
        #endregion

        public override object Clone()
            => new Squeezing();
    }
}
