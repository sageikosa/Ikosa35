using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class TroveDamageHandler : IInteractHandler
    {
        public static readonly TroveDamageHandler Static = new TroveDamageHandler();
      
        public IEnumerable<Type> GetInteractionTypes()
        {
            // most likely falling damage only...
            yield return typeof(DeliverDamageData);

            // just in case some saveable damage gets to the trove, make sure contents get it...
            yield return typeof(SaveableDamageData);
            yield break;
        }

        public void HandleInteraction(Interaction workSet)
        {
            // collect from damage
            if ((workSet?.InteractData is IDeliverDamage _damage)
                && (workSet?.Target is ICapturePassthrough _pass))
            {
                foreach (var _obj in _pass.Contents.OfType<ICoreObject>())
                {
                    var _dmg = _damage.GetClone();
                    _obj.HandleInteraction(new Interaction(null, workSet.Source, _obj, _dmg));
                }
                workSet.Feedback.Add(new UnderstoodFeedback(this));
            }
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return false;
        }
    }
}
