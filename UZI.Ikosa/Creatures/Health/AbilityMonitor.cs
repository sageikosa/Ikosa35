using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa
{
    /// <summary>Ensures conditions match ability scores</summary>
    [Serializable]
    public class AbilityMonitor : ICreatureBound, IMonitorChange<DeltaValue>
    {
        /// <summary>Ensures conditions match ability scores</summary>
        public AbilityMonitor(Creature creature)
        {
            _Critter = creature;
            foreach (var _ability in Creature.Abilities.AllAbilities)
                _ability.AddChangeMonitor(this);
        }

        #region data
        private Creature _Critter;
        #endregion

        public Creature Creature => _Critter;

        public void PreTestChange(object sender, AbortableChangeEventArgs<DeltaValue> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args) { }

        public void ValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
            if (sender is AbilityBase _ability)
            {
                var _zero = !_ability.IsNonAbility && (_ability.EffectiveValue == 0);
                if (sender is Strength)
                {
                    // IMMOBILIZED
                    var _immobilized = Creature.Adjuncts.OfType<Immobilized>()
                        .FirstOrDefault(_d => _d.Source == sender);
                    if (_zero && _immobilized == null)
                    {
                        // need to turn on
                        Creature.AddAdjunct(new Immobilized(sender, true));
                    }
                    else if (!_zero && (_immobilized != null))
                    {
                        // need to turn off
                        _immobilized.Eject();
                    }
                }
                else if (sender is Dexterity)
                {
                    // IMMOBILIZED
                    var _immobilized = Creature.Adjuncts.OfType<Immobilized>()
                        .FirstOrDefault(_d => _d.Source == sender);
                    if (_zero && _immobilized == null)
                    {
                        // need to turn on
                        Creature.AddAdjunct(new Immobilized(sender, false));
                    }
                    else if (!_zero && (_immobilized != null))
                    {
                        // need to turn off
                        _immobilized.Eject();
                    }
                }
                else if (sender is Constitution)
                {
                    // DEAD
                    var _dead = Creature.Adjuncts.OfType<DeadEffect>().FirstOrDefault(_d => _d.Source == sender);
                    if (_zero && _dead == null)
                    {
                        // need to turn on
                        Creature.AddAdjunct(new DeadEffect(sender, (Creature.Setting as ITacticalMap)?.CurrentTime ?? 0));
                    }
                    else if (!_zero && (_dead != null))
                    {
                        // need to turn off
                        _dead.Eject();
                    }
                }
                else if (sender is Intelligence)
                {
                    // UNCONSCIOUS
                    var _unconscious = Creature.Adjuncts.OfType<UnconsciousEffect>()
                        .FirstOrDefault(_d => _d.Source == sender);
                    if (_zero && _unconscious == null)
                    {
                        // need to turn on
                        Creature.AddAdjunct(new UnconsciousEffect(sender, double.MaxValue, Round.UnitFactor));
                    }
                    else if (!_zero && (_unconscious != null))
                    {
                        // need to turn off
                        _unconscious.Eject();
                    }
                }
                else if (sender is Wisdom)
                {
                    // UNCONSCIOUS (deep sleep with nightmares)
                    var _unconscious = Creature.Adjuncts.OfType<UnconsciousEffect>()
                        .FirstOrDefault(_d => _d.Source == sender);
                    if (_zero && _unconscious == null)
                    {
                        // need to turn on
                        Creature.AddAdjunct(new UnconsciousEffect(sender, double.MaxValue, Round.UnitFactor));
                    }
                    else if (!_zero && (_unconscious != null))
                    {
                        // need to turn off
                        _unconscious.Eject();
                    }
                }
                else if (sender is Charisma)
                {
                    // UNCONSCIOUS
                    var _unconscious = Creature.Adjuncts.OfType<UnconsciousEffect>()
                        .FirstOrDefault(_d => _d.Source == sender);
                    if (_zero && _unconscious == null)
                    {
                        // need to turn on
                        Creature.AddAdjunct(new UnconsciousEffect(sender, double.MaxValue, Round.UnitFactor));
                    }
                    else if (!_zero && (_unconscious != null))
                    {
                        // need to turn off
                        _unconscious.Eject();
                    }
                }
            }
        }
    }
}
