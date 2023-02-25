using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Creatures.Templates;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Fidelity;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// Base-class used for permanent negative levels (RaiseDead).  
    /// Derived classes include TransientNegativeLevel (Enervation) and PersistentNegativeLevel (EnergyDrain)
    /// </summary>
    [Serializable]
    public class NegativeLevel : Adjunct
    {
        /// <summary>
        /// Base-class used for permanent negative levels (RaiseDead).  
        /// Derived classes include TransientNegativeLevel (Enervation) and PersistentNegativeLevel (EnergyDrain)
        /// </summary>
        public NegativeLevel(object source)
            : base(source)
        {
            // NOTE: negative level penalties stack
            _Penalty = new Delta(-1, this);
            _Riser = typeof(Wight);
        }

        private Delta _Penalty;
        protected Type _Riser;

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (Anchor is Creature _critter)
            {
                _critter.HealthPoints.ExtraHealthPoints -= 5;
                _critter.ExtraAbilityCheck.Deltas.Add(_Penalty);
                _critter.MeleeDeltable.Deltas.Add(_Penalty);
                _critter.RangedDeltable.Deltas.Add(_Penalty);
                _critter.OpposedDeltable.Deltas.Add(_Penalty);
                _critter.FortitudeSave.Deltas.Add(_Penalty);
                _critter.WillSave.Deltas.Add(_Penalty);
                _critter.ReflexSave.Deltas.Add(_Penalty);
                _critter.ExtraSkillCheck.Deltas.Add(_Penalty);
                _critter.ExtraClassPowerLevel.Deltas.Add(_Penalty);
                if ((_critter.ExtraClassPowerLevel.EffectiveValue + _critter.AdvancementLog.NumberPowerDice) <= 0)
                {
                    // dead
                    var _now = _critter?.GetCurrentTime() ?? 0;
                    _critter.AddAdjunct(new DeadEffect(this, _now));

                    // timer to rise (random number of hours 3d4...)
                    var _riseTime = _now + DiceRoller.RollDice(_critter.ID, 3, 4, @"Rise as Wight", (new Hour()).PluralName) * Hour.UnitFactor;
                    _critter.AddAdjunct(new RisingDrainee(_Riser, _riseTime));
                }
            }
        }
        #endregion

        protected override void OnDeactivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                _critter.HealthPoints.ExtraHealthPoints += 5;
            }
            _Penalty.DoTerminate();
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new NegativeLevel(Source);
    }
}
