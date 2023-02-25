using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Feats
{
    [Serializable]
    public class CleaveReactor : Adjunct, ISecondaryAttackResult
    {
        public CleaveReactor(CleaveFeat cleave)
            : base(cleave)
        {
        }

        #region public CleaveBudget GetCleaveBudget()
        public CleaveBudget GetCleaveBudget()
        {
            if (CleaveFeat.Creature.GetLocalActionBudget() is LocalActionBudget _budget)
            {
                if (_budget.BudgetItems[typeof(CleaveBudget)] is CleaveBudget _cleaveBudget)
                {
                    return _cleaveBudget;
                }
                else
                {
                    _cleaveBudget = new CleaveBudget(CleaveFeat.Capacity);
                    _budget.BudgetItems.Add(typeof(CleaveBudget), _cleaveBudget);
                    return _cleaveBudget;
                }
            }
            return new CleaveBudget(CleaveFeat.Capacity);
        }
        #endregion

        public CleaveFeat CleaveFeat => Source as CleaveFeat;
        public IWeaponHead WeaponHead => Anchor as IWeaponHead;
        public IMeleeWeapon Weapon => WeaponHead?.ContainingWeapon as IMeleeWeapon;

        public override object Clone()
            => new CleaveReactor(CleaveFeat);

        public object AttackResultSource => CleaveFeat;

        public bool PoweredUp { get => true; set { } }

        public void AttackResult(StepInteraction deliverDamageInteraction)
        {
            // find all creatures in strike zone of weaponHead (excluding self and original target)
            var _critter = CleaveFeat.Creature;
            var _map = _critter.Setting as LocalMap;
            var _target = deliverDamageInteraction.Target;

            // get geometry as a snapshot
            var _rgn = Weapon.GetStrikeZone(true)?.Region;
            var _wLoc = Weapon.GetLocated()?.Locator;
            if ((_rgn != null) && (_map != null) && (_wLoc != null))
            {
                var _all = (from _loc in _map.MapContext.LocatorsInRegion(_rgn, _wLoc.PlanarPresence)
                            let _chief = _loc.Chief
                            where _chief != null
                            // must be aware to prompt
                            && _critter.Awarenesses.GetAwarenessLevel(_chief.ID) >= Senses.AwarenessLevel.Aware
                            // cannot be self or original target
                            && _chief != _critter && _critter != _target
                            select _chief).ToList();
                if (_all.Any())
                {
                    var _next = new CleaveAttackCheck(deliverDamageInteraction.Step, this, _all);
                    deliverDamageInteraction.Step.AppendFollowing(_next);
                }
            }
        }

        public IEnumerable<StepPrerequisite> AttackResultPrerequisites(Interaction workSet)
        {
            yield break;
        }

        #region public bool IsDamageSufficient(StepInteraction final)
        public bool IsDamageSufficient(StepInteraction final)
        {
            //  budget check: make sure we have enough left
            if (GetCleaveBudget().Available > 0)
            {
                var _total = (final?.InteractData as IDeliverDamage)?.GetTotal() ?? 0;
                if (final.Target is Creature _critter)
                {
                    var _current = _critter.HealthPoints.CurrentValue - _critter.HealthPoints.NonLethalDamage;
                    if ((_current < 0) && ((_current + _total) > 0))
                    {
                        // this attack was enough to "drop" the creature
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion
    }
}
