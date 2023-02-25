using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Items.Weapons;
using System.Collections.Generic;

namespace Uzi.Ikosa.Deltas
{
    /// <summary>Only applied during two weapon fighting actions.</summary>
    [Serializable]
    public class MultiWeaponDelta : IQualifyDelta
    {
        #region construction
        /// <summary>Only applied during two weapon fighting actions.</summary>
        public MultiWeaponDelta()
        {
            _Term = new TerminateController(this);
            _Main = new ConstDeltable(-6);
            _Off = new ConstDeltable(-10);
            _Light = new ConstDeltable(2);
        }
        #endregion

        #region data
        private readonly TerminateController _Term;
        private readonly ConstDeltable _Main;
        private readonly ConstDeltable _Off;
        private readonly ConstDeltable _Light;
        #endregion

        public ConstDeltable MainHandPenalty => _Main;
        public ConstDeltable OffHandPenalty => _Off;
        public ConstDeltable OffHandLightWeaponBonus => _Light;  // TODO: qualified delta?

        #region IQualifyDelta Members

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            // valid interaction?
            if (qualify is Interaction _iAct)
            {
                // attack with some kind of weapon head?
                if ((_iAct.InteractData is AttackData) && (qualify.Source is IWeaponHead _head))
                {
                    // natural weapons do are not affected by off-hand penalties
                    if (_head.ContainingWeapon is NaturalWeapon)
                        yield break;

                    // see if there are any non-light off-hand weapons
                    var _critter = qualify.Actor as Creature;
                    var _heavyOff = (from _wpn in _critter.Body.ItemSlots.HeldObjects.OfType<IWeapon>()
                                     where _wpn.IsActive && !_wpn.IsLightWeapon
                                     select _wpn).Any();

                    // is the attack-head associated with a "main" hand?
                    if (!_head.IsOffHand)
                    {
                        // head associated with a main hand
                        yield return new QualifyingDelta(MainHandPenalty.EffectiveValue + (_heavyOff ? 0 : OffHandLightWeaponBonus.EffectiveValue),
                             typeof(MultiWeaponDelta), @"Multi-Weapon");
                    }
                    else
                    {
                        // head associated with only off-hands
                        yield return new Delta(OffHandPenalty.EffectiveValue + (_heavyOff ? 0 : OffHandLightWeaponBonus.EffectiveValue),
                              typeof(MultiWeaponDelta), @"Multi-Weapon Off Hand");
                    }
                }
            }
            yield break;
        }

        #endregion

        #region IControlTerminate Members

        public void DoTerminate()
        {
            _Term.DoTerminate();
        }

        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Term.TerminateSubscriberCount;

        #endregion
    }
}
