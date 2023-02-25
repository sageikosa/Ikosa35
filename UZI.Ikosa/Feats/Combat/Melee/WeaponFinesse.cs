using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Items.Shields;
using System.Collections.Generic;

namespace Uzi.Ikosa.Feats
{
    /// <summary>
    /// Automatically uses dexterity (and shield check penalty) as substitute strength modifier on melee attack rolls 
    /// with finessable weapons if the combined value is better than the normal strength modifier.
    /// </summary>
    [Serializable,
    BaseAttackRequirement(1),
    FighterBonusFeat,
    FeatInfo(@"Weapon Finesse")
    ]
    public class WeaponFinesse : FeatBase, IQualifyDelta
    {
        public WeaponFinesse(object source, int powerLevel)
            : base(source, powerLevel)
        {
            _Term = new TerminateController(this);
        }

        public override string Benefit
        {
            get { return @"Attack with finessable weapons use dexterity modifier instead of strength (if better).  Check penalty of shield applies to attack rolls."; }
        }

        protected override void OnActivate()
        {
            Creature.MeleeDeltable.Deltas.Add((IQualifyDelta)this);
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            DoTerminate();
            base.OnDeactivate();
        }

        #region IQualifyDelta Members

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if (qualify is Interaction _iAct)
            {
                // melee attack
                if (_iAct.InteractData is MeleeAttackData _melee)
                {
                    if (qualify.Source is IWeaponHead _head)
                    {
                        if ((_head.ContainingWeapon is IMeleeWeapon _weapon) && _weapon.IsFinessable)
                        {
                            var _delta = Creature.Abilities.Dexterity.IModifier.Value;

                            // check penalties are negative, so the lowest (first ascending) is the worst
                            // if there are none, _shld == 0
                            var _shld = (from _s in Creature.Body.ItemSlots.HeldObjects.OfType<ShieldBase>()
                                         let _check = _s.CheckPenalty.QualifiedValue(qualify)
                                         orderby _check
                                         select _check).FirstOrDefault();
                            _delta += _shld;

                            var _str = Creature.Abilities.Strength.IModifier.Value;
                            if (_delta > _str)
                            {
                                if (_str < 0)
                                {
                                    // if STR delta < 0, provide DEX delta to overcome strength and apply DEX
                                    yield return new QualifyingDelta(_delta + (0 - _str), typeof(Dexterity), @"Weapon Finesse override STR");
                                }
                                else
                                {
                                    yield return new QualifyingDelta(_delta, typeof(Strength), @"Weapon Finesse replace STR");
                                }
                            }
                        }
                    }
                }
            }
            yield break;
        }

        #endregion

        #region IControlTerminate Members
        private readonly TerminateController _Term;
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
