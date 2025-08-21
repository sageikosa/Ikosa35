using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Contracts;
using System.Collections.Generic;

namespace Uzi.Ikosa.Abilities
{
    /// <summary>Qualified delta to apply strength damage on a melee attack, or thrown weapon attack</summary>
    [Serializable]
    public class MeleeStrengthDamage : IQualifyDelta, ICreatureBound
    {
        public MeleeStrengthDamage(Creature critter)
        {
            _Critter = critter;
            _Terminator = new TerminateController(this);
        }

        #region ITerminating Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  
        /// Note: this does not destroy the modifier and it can be re-used.
        /// </summary>
        public void DoTerminate()
        {
            _Terminator.DoTerminate();
        }

        #region IControlTerminate Members
        private readonly TerminateController _Terminator;
        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Terminator.TerminateSubscriberCount;
        #endregion
        #endregion

        private Creature _Critter;
        public Creature Creature { get { return _Critter; } }

        public virtual IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if (qualify.Source as IWeaponHead == null)
            {
                yield break;
            }

            // ranged attacks use strength or half strength
            if (!(qualify is Interaction _iAct))
            {
                yield break;
            }

            var _ranged = _iAct.InteractData is RangedAttackData;

            if ((qualify.Source as IWeaponHead).ContainingWeapon is NaturalWeapon)
            {
                var _natrl = (qualify.Source as IWeaponHead).ContainingWeapon as NaturalWeapon;

                // secondary natural weapons get only half STR damage
                if (!_natrl.IsPrimary)
                {
                    yield break;
                }

                // sole natural weapons get 1.5 STR damage
                if ((Creature.Body.NaturalWeapons.Count == 1) || _natrl.TreatAsSoleWeapon)
                {
                    yield break;
                }
            }
            else
            {
                // not a melee weapon (or thrown melee)
                if (!((qualify.Source as IWeaponHead).ContainingWeapon is IMeleeWeapon))
                {
                    yield break;
                }

                // if not thrown, check wield templates
                if (!_ranged)
                {
                    // off hand weapons get half STR damage
                    if ((qualify.Source as IWeaponHead).IsOffHand)
                    {
                        yield break;
                    }

                    // must have at least one main slot
                    switch ((qualify.Source as IWeaponHead).ContainingWeapon.GetWieldTemplate())
                    {
                        case WieldTemplate.OneHanded:
                        case WieldTemplate.TwoHanded:
                        case WieldTemplate.Double:
                            // two-hand wielding these types gets 1.5 STR damage
                            if ((qualify.Source as IWeaponHead).AssignedSlots.Count() == 2)
                            {
                                yield break;
                            }

                            break;
                    }
                }
            }

            // conditional STR boosts (lifting) won't affect DeltaValue
            yield return new QualifyingDelta(Creature.Abilities.Strength.DeltaValue, typeof(Strength), @"Strength");
            yield break;
        }
    }
}
