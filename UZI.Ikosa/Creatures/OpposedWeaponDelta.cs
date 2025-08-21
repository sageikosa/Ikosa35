using System;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Contracts;
using System.Collections.Generic;

namespace Uzi.Ikosa
{
    /// <summary>Qualified Delta to apply to opposed weapon rolls (Disarm and Sunder)</summary>
    [Serializable]
    public class OpposedWeaponDelta : IQualifyDelta, ISourcedObject
    {
        /// <summary>Qualified Delta to apply to opposed weapon rolls (Disarm and Sunder)</summary>
        public OpposedWeaponDelta(Creature critter)
        {
            _Term = new TerminateController(this);
            _Critter = critter;
        }

        #region data
        private TerminateController _Term;
        private Creature _Critter;
        #endregion

        #region IQualifyDelta Members

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if (qualify is Interaction _iAct)
            {
                if ((_iAct.InteractData is SunderWieldedItemData) || (_iAct.InteractData is DisarmData))
                {
                    var _target = qualify.Target as IWeapon;
                    if (qualify.Source is IWeaponHead _head)
                    {
                        #region get _wpn
                        // find opposed weapon head for the creature that owns this delta
                        IWeapon _wpn = null;
                        if ((_head.ContainingWeapon.CreaturePossessor == Creature) && _head.ContainingWeapon.IsActive)
                        {
                            _wpn = _head.ContainingWeapon;
                        }
                        else if ((_target != null) && (_target.CreaturePossessor == Creature) && _target.IsActive)
                        {
                            _wpn = _target;
                        }
                        #endregion

                        if (_wpn != null)
                        {
                            var _nonMelee = (!(_wpn is IMeleeWeapon) ? -4 : 0);

                            if (_iAct.InteractData is SunderWieldedItemData)
                            {
                                #region Sunder Delta
                                // sunder rolls are determined by weapon's category
                                switch (_wpn.GetWieldTemplate())
                                {
                                    case WieldTemplate.TwoHanded:
                                        yield return new QualifyingDelta(4 + _nonMelee, typeof(OpposedWeaponDelta), @"Two Handed Weapon");
                                        break;

                                    case WieldTemplate.OneHanded:
                                    case WieldTemplate.Double:
                                        if (_nonMelee != 0)
                                        {
                                            yield return new QualifyingDelta(_nonMelee, typeof(OpposedWeaponDelta), @"Non-melee weapon");
                                        }

                                        break;

                                    default: // light/unarmed
                                        yield return new QualifyingDelta(-4 + _nonMelee, typeof(OpposedWeaponDelta), @"Light Weapon");
                                        break;
                                }
                                #endregion
                            }
                            else
                            {
                                #region Disarm Delta
                                // disarm rolls are determined by grip style
                                if ((_wpn.SecondarySlot != null)
                                    && _wpn.GetWieldTemplate().NotIn(WieldTemplate.Unarmed, WieldTemplate.Light))
                                {
                                    if ((!(_wpn is DoubleMeleeWeaponBase _dbl)) || _dbl.UseAsTwoHanded)
                                    {
                                        // two-hands on a weapon which can generally benefit from 2 hands
                                        yield return new QualifyingDelta(4 + _nonMelee, typeof(OpposedWeaponDelta), @"Two Hand Holding");
                                    }
                                    else if (_nonMelee != 0)
                                    {
                                        yield return new QualifyingDelta(_nonMelee, typeof(OpposedWeaponDelta), @"Non-melee weapon");
                                    }
                                }
                                else
                                {
                                    // only 1 hand, or a weapon that can't get benefits from two hands
                                    if (_wpn.GetWieldTemplate().In(WieldTemplate.Unarmed, WieldTemplate.Light))
                                    {
                                        // such as a light weapon
                                        yield return new QualifyingDelta(-4 + _nonMelee, typeof(OpposedWeaponDelta), @"Light Weapon");
                                    }
                                    else
                                    {
                                        // but, if the weapon is not a melee weapon, take a penalty
                                        if (_nonMelee != 0)
                                        {
                                            yield return new QualifyingDelta(_nonMelee, typeof(OpposedWeaponDelta), @"Non-melee weapon");
                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            // general non-melee delta (will also likely get a non-proficiency delta)
                            yield return new QualifyingDelta(-4, typeof(OpposedWeaponDelta), @"Non-melee weapon");
                        }
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

        public object Source { get { return _Critter; } }
        public Creature Creature { get { return _Critter; } }
    }
}
