using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Items.Weapons
{
    public class GetUnarmedWeapon : InteractData
    {
        public GetUnarmedWeapon(CoreActor actor)
            : base(actor)
        {
        }

        private static GetUnarmedWeaponHandler _Static = new GetUnarmedWeaponHandler();

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            yield return _Static;
            yield break;
        }
    }

    public class GetUnarmedWeaponFeedback : InteractionFeedback
    {
        public GetUnarmedWeaponFeedback(object source)
            : base(source)
        {
        }

        public IMeleeWeapon Weapon { get; set; }
    }

    public class GetUnarmedWeaponHandler : IInteractHandler
    {
        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet != null)
            {
                if (workSet.Actor is Creature _critter)
                {
                    var _unarmed = new UnarmedWeapon()
                    {
                        Possessor = _critter
                    };
                    workSet.Feedback.Add(new GetUnarmedWeaponFeedback(this) { Weapon = _unarmed });
                }
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(GetUnarmedWeapon);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // default
            return false;
        }

        #endregion
    }
}
