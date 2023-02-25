using System;
using System.Linq;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// Controls alterations to the attack roll by virtue (or vice) of the defender's intrinsic conditions
    /// </summary>
    [Serializable]
    public class ConditionAttackHandler : IInteractHandler
    {
        #region public void HandleInteraction(InteractionWorkSet workSet)
        public void HandleInteraction(Interaction workSet)
        {
            /*
            ATK modifier
            DEFENDER IS                 Melee   Ranged
            Blinded                     +2(1)   +2(1) 
            Cowering                    +2(1)   +2(1) 
            Unprepared to dodge (such as surprised, balancing, climbing) 
                                        +0(1)   +0(1) 
            Grappling (but attacker is not) 
                                        +0(1)   +0(1,3)
            Helpless (such as paralyzed, sleeping, or bound) 
                                        +4(4)   +0(4)
            Kneeling or sitting         +2      -2 
            Pinned                      +4(4)   +0(4 )
            Prone                       +4      -4 
            Squeezing through a space   +4      +4 
            Stunned                     +2(1)   +2(1)
             * 
            (1) The defender loses any Dexterity bonus to AR. 
            (2) An entangled character takes a -4 penalty to Dexterity. 
            (3) Roll randomly to see which grappling combatant you strike. That defender loses any Dexterity bonus to AR. 
            (4) Treat the defender’s Dexterity as 0 (-5 modifier). Rogues can sneak attack helpless or pinned defenders. 
             */

            var _rAtk = workSet.InteractData as RangedAttackData;
            if ((workSet.InteractData is AttackData _atk)
                && (workSet.Target is Creature _target))
            {
                // common tests and results
                bool _is(string condition)
                    => _target.Conditions.Contains(condition);
                void _alter(InteractionAlteration alteration)
                    => workSet.InteractData.Alterations.Add(workSet.Target, alteration);

                // attacker invisible
                if (_atk.Attacker != null)
                {
                    if (_target.Awarenesses[_atk.Attacker.ID] < AwarenessLevel.Aware)
                    {
                        var _unaware = new TargetUnawareAlteration(_atk);
                        _alter(_unaware);
                        _alter(new MaxDexterityToARAlteration(_atk, (IDelta)_target.MaxDexterityToARBonus, _unaware));
                    }
                }
                else
                {
                    // unable to see trap?
                    // TODO:
                }

                // TODO: kneeling/sitting (tactical?)

                if (_is(Condition.Helpless))
                {
                    // if not a ranged attack (ie, melee or reach) give a boost to the attack
                    if (_rAtk == null)
                    {
                        _alter(new TargetConditionAlteration(_atk, Condition.Helpless, 4, true));
                    }
                    _alter(new TargetDexterityZeroAlteration(_atk, _target));
                }
                else if (_is(Condition.Grappling))
                {
                    if (!_target.IsGrappling(_atk.Attacker.ID))
                    {
                        // target grappling, attacker not grappling target, attacker overcomes max dex
                        _alter(new MaxDexterityToARAlteration(_atk, (IDelta)_target.MaxDexterityToARBonus, typeof(Grappler)));
                    }
                    if (_is(Condition.Pinned))
                    {
                        if (!_target.Adjuncts.OfType<Pinnee>().Any(_p => _p.PinGroup.Pinner.ID == _atk.Attacker.ID))
                        {
                            // target pinned, attacker not the pinner, attacker gets +4
                            _alter(new TargetConditionAlteration(_atk, Condition.Pinned, 4, true));
                        }
                    }
                }
                else
                {
                    // can be both prone and stunned
                    if (_is(Condition.Prone))
                    {
                        // prone hinders melee/reach AR, but helps ranged
                        _alter((_rAtk == null)
                            ? new TargetConditionAlteration(_atk, Condition.Prone, 4, true)
                            : new TargetConditionAlteration(_atk, Condition.Prone, -4, true));
                    }

                    if (_is(Condition.Cowering))
                    {
                        // cowering loses DEX to AR
                        var _tc = new TargetConditionAlteration(_atk, Condition.Cowering, 2, true);
                        _alter(_tc);
                        _alter(new MaxDexterityToARAlteration(_atk, (IDelta)_target.MaxDexterityToARBonus, _tc));
                    }
                    else if (_is(Condition.Stunned))
                    {
                        // stunned loses DEX to AR
                        var _tc = new TargetConditionAlteration(_atk, Condition.Stunned, 2, true);
                        _alter(_tc);
                        _alter(new MaxDexterityToARAlteration(_atk, (IDelta)_target.MaxDexterityToARBonus, _tc));
                    }
                    else if (_is(Condition.UnpreparedToDodge))
                    {
                        _alter(new MaxDexterityToARAlteration(_atk, (IDelta)_target.MaxDexterityToARBonus, typeof(UnpreparedToDodge)));
                    }
                }
            }

            return;
        }
        #endregion

        #region public IEnumerable<Type> GetInteractionTypes()
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(MeleeAttackData);
            yield return typeof(ReachAttackData);
            yield return typeof(RangedAttackData);
            yield break;
        }
        #endregion

        #region public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // put this after the transit handler
            var _type = existingHandler.GetType();
            if ((_type == typeof(TransitAttackHandler)) || (_type == typeof(FlankingCheckHandler)))
                return false;
            else
                return true;
        }
        #endregion
    }
}
