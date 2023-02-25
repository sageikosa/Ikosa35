using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize;
using Uzi.Core;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Actions;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class SneakAttack : Adjunct, IProcessFeedback
    {
        public SneakAttack(IPowerClass powerClass, int baseLevel, int levelIncrement)
            : base(powerClass)
        {
            _Base = baseLevel;
            _Increment = levelIncrement;
        }

        #region data
        private int _Base;
        private int _Increment;
        #endregion

        public int BaseLevel => _Base;
        public int LevelIncrement => _Increment;

        public IPowerClass PowerClass
            => Source as IPowerClass;

        public override object Clone()
            => new SneakAttack(PowerClass, BaseLevel, LevelIncrement);

        // IProcessFeedback Members
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(GetDamageRollPrerequisites);
            yield break;
        }

        public void HandleInteraction(Interaction workSet)
        {
            /* NOP */
        }

        public virtual bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => true;

        #region protected virtual bool InRange(RangedAttackData rangedAtk, Creature target)
        /// <summary>True if not a ranged attack, or if a ranged attack is in 30 feet</summary>
        protected virtual bool InRange(RangedAttackData rangedAtk, Creature target)
        {
            if (rangedAtk != null)
            {
                // Ranged sneak attacks only within 30 feet
                var _loc = target.GetLocated()?.Locator;
                if (_loc == null)
                {
                    // cannot determine location
                    return false;
                }
                if (IGeometricHelper.NearDistance(_loc.GeometricRegion, rangedAtk.AttackPoint) > 30d)
                {
                    // too far away
                    return false;
                }
            }

            // not a ranged attack or in range
            return true;
        }
        #endregion

        /// <summary>
        /// <para>Undead, constructs, oozes, plants, and incorporeal creatures are immune.</para>
        /// <para>Creatures immune to criticals are immune to sneak attacks.</para>
        /// </summary>
        protected virtual bool CanSneakAttackCreature(Creature critter)
            => critter.CreatureType.IsLiving
            && !critter.IsImmuneToCriticals
            && critter.Body.HasAnatomy
            && !(critter.CreatureType is PlantType)
            && !(critter.CreatureType is OozeType)
            && !critter.HasActiveAdjunct<Incorporeal>();

        public void ProcessFeedback(Interaction workSet)
        {
            if (workSet?.InteractData is GetDamageRollPrerequisites)
            {
                var _getDmgRollPre = workSet.InteractData as GetDamageRollPrerequisites;
                var _atkInteract = _getDmgRollPre.Interaction;
                if (_atkInteract?.Target is Creature _target)
                {
                    if (CanSneakAttackCreature(_target))
                    {
                        var _atk = _getDmgRollPre.AttackData;

                        // Must see target: cannot sneak attack a creature with concealment (relatively...)
                        if (_atk.Alterations.OfType<ConcealmentAlteration>().Any()
                            // no sneak attack if not in range
                            || !InRange(_atk as RangedAttackData, _target)
                            // only sap or an unarmed strike can sneak attack with nonlethal damage
                            || (_atk.IsNonLethal
                            && !(_atkInteract.Source is Sap) && !(_atkInteract.Source is UnarmedWeapon))
                            )
                        {
                            return;
                        }
                        else
                        {
                            // Target would be denied Dexterity to AR, or is flanked
                            if (_atk.Alterations.OfType<MaxDexterityToARAlteration>().Any()
                                || _atk.Alterations.OfType<FlankingAlteration>().Any())
                            {
                                // Must reach bulk of creature (not limbs only) !!!
                                // TODO:

                                // Extra damage = 1d6 @ base power-level, and increases +1d6 every increment levels
                                var _curr = PowerClass.ClassPowerLevel.QualifiedValue(workSet);
                                var _dice =
                                    (_curr >= BaseLevel)
                                    ? 1 + ((_curr - BaseLevel) / LevelIncrement)
                                    : 0;
                                workSet.Feedback.Add(new ValueFeedback<DamageRollPrerequisite>(this,
                                    new DamageRollPrerequisite(this, _atkInteract, @"SneakAttack", @"Sneak Attack",
                                    new DiceRoller(_dice, 6), false, _atk.IsNonLethal, @"Sneak", 0)));
                            }
                        }
                    }
                }
            }
        }
    }
}
