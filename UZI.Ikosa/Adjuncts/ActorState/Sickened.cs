using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Sickened : Adjunct
    {
        public Sickened(object source)
            : base(source)
        {
            _Penalty = new Delta(-2, typeof(Sickened), @"Sickened");
        }

        private Delta _Penalty;

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (Anchor is Creature _critter)
            {
                // condition add
                _critter.Conditions.Add(new Condition(Condition.Sickened, this));

                // add penalty: -2 all attack rolls, weapon damage rolls, saving throws, skill checks, and ability checks.
                _critter.MeleeDeltable.Deltas.Add(_Penalty);
                _critter.RangedDeltable.Deltas.Add(_Penalty);
                _critter.OpposedDeltable.Deltas.Add(_Penalty);
                _critter.ExtraWeaponDamage.Deltas.Add(_Penalty);
                _critter.FortitudeSave.Deltas.Add(_Penalty);
                _critter.ReflexSave.Deltas.Add(_Penalty);
                _critter.WillSave.Deltas.Add(_Penalty);
                _critter.ExtraSkillCheck.Deltas.Add(_Penalty);
                _critter.ExtraAbilityCheck.Deltas.Add(_Penalty);
            }
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                // remove penalty
                _Penalty.DoTerminate();

                // condition remove
                _critter.Conditions.Remove(_critter.Conditions[Condition.Sickened, this]);
            }
            base.OnDeactivate(source);
        }
        #endregion

        public override object Clone()
        {
            return new Sickened(Source);
        }
    }
}
