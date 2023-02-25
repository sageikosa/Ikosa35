using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Magic.Spells;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Incorporeal : Adjunct, IInteractHandler
    {
        // TODO: !!!
        public Incorporeal(object source)
            : base(source)
        {
        }

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            (Anchor as CoreObject)?.AddIInteractHandler(this);
            // TODO: incorporeal movement: move through solid matter, one face must remain in open-space
            // TODO: no physical body (void material?)
            // TODO: incorporeal can "HARM" incorporeal !!!
            // TODO: immune to non-magical attacks
            // TODO: magic weapons, spells, spell-like, and super-natural can affect, and holy-water on undead
            // TODO: 50% change to ignore damage from corporeal source
            // TODO: 50% exclusions (ie, full-effect): positive/negative/force/ghost-touch
            // TODO: range of touch sense: presence, but total-conceal 50% miss-chance if not able to see target
            // TODO: attacking creature from inside solid-cell has cover, otherwise total-cover if inside solid-cell
            // TODO: ignore natural armor, armor, shields; deflection and force-affects still apply against attacks
            // TODO: water treated like air
            // TODO: cannot fall, cannot trip, cannot grapple
            // TODO: silent (unless desired to be heard)
            // TODO: DEX instead of STR of all attacks
        }

        protected override void OnDeactivate(object source)
        {
            // TODO: incorporeal movement
            (Anchor as CoreObject)?.RemoveIInteractHandler(this);
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new Incorporeal(Source);

        // IInteractHandler
        public void HandleInteraction(Interaction workSet)
        {
            var _target = workSet?.Target as ICoreObject;
            switch (workSet?.InteractData)
            {
                case AttackData _atk:
                    if (_atk?.Attacker.PathHasActiveAdjunct<Incorporeal>() ?? false)
                    {
                        if ((_atk.Attacker.PathHasActiveAdjunct<EtherealState>() == _target.PathHasActiveAdjunct<EtherealState>()))
                        {
                            return;
                        }
                    }
                    if (workSet.Source is IAttackSource _atkSrc)
                    {
                        // TODO: handle various attack sources...
                        if (_atkSrc.Adjuncts.Anchor is ICoreObject _cObj)
                        {
                            //if (_cObj.CanMateriallyInteract(_target)        // compatible
                            //    || _atkSrc.IsEnhancedActive()               // augmented
                            //    || _atkSrc.IsMagicWeaponActive()            // magic effect
                            //    || _atkSrc.HasActiveAdjunct<GhostTouchWeapon>()
                            //    )
                            //{
                            //    // attack source compatible, do not block
                            //    return;
                            //}
                        }
                    }
                    workSet.Feedback.Add(new AttackFeedback(this, false));
                    break;

                default:
                    break;
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            // TODO: other damage/interactions...
            yield return typeof(MeleeAttackData);
            yield return typeof(ReachAttackData);
            yield return typeof(RangedAttackData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => true;
    }
}
